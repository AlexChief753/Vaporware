using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Game/Shop/Shop Service")]
public class ShopService : MonoBehaviour
{
    public static ShopService Instance { get; private set; }

    [Header("Config")]
    public ItemPoolSO pool;
    [Range(1, 8)] public int itemsPerShop = 4;

    // State for the inter-level shop
    private int _selectionLevel = -1;       // which GameGrid.level this selection belongs to
    private readonly List<Item> _selection = new();   // size itemsPerShop
    private readonly HashSet<string> _soldNames = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static ShopService FindOrCreate()
    {
        if (Instance) return Instance;

        // Prefer an already-placed ShopService in the scene, even if it hasn't Awakened yet
        var existing = FindFirstObjectByType<ShopService>(FindObjectsInactive.Include);
        if (existing != null)
        {
            Instance = existing;
            return Instance;
        }

        // create a new ShopService if not in scene
        var go = new GameObject("~ShopService");
        var svc = go.AddComponent<ShopService>();
        Instance = svc;
        return svc;
    }


    public void EnsureSelectionForCurrentLevel()
    {
        int current = Mathf.Max(1, GameGrid.level);
        if (_selectionLevel == current && _selection.Count == itemsPerShop)
        {
            bool allNull = true;
            for (int i = 0; i < _selection.Count; i++)
            {
                if (_selection[i] != null) { allNull = false; break; }
            }

            // If it's a good selection, keep it
            if (!allNull) return;

            // Otherwise fall through to rebuild below
        }

        // New level (or first time): roll a fresh set
        _selectionLevel = current;
        _selection.Clear();
        _soldNames.Clear();

        if (pool == null || pool.entries == null)
        {
            Debug.LogWarning("[ShopService] No pool assigned; shop will be empty.");
            PadSelectionWithNulls();
            return;
        }

        // Weighted sample without replacement
        var candidates = new List<ItemPoolSO.Entry>(pool.entries);
        for (int k = 0; k < itemsPerShop; k++)
        {
            var picked = PickWeighted(candidates);
            _selection.Add(picked != null ? picked.item : null);
            if (picked != null) candidates.Remove(picked); // no replacement
        }

        // keep length
        while (_selection.Count < itemsPerShop) _selection.Add(null);


    }

    private void PadSelectionWithNulls()
    {
        while (_selection.Count < itemsPerShop) _selection.Add(null);
    }

    private ItemPoolSO.Entry PickWeighted(List<ItemPoolSO.Entry> list)
    {
        int total = 0;
        foreach (var e in list)
        {
            int baseW = pool.WeightFor(e);
            // scale by active character's rarity multiplier
            baseW = Mathf.RoundToInt(baseW * CharacterEffectsManager.GetRarityWeightMultiplier(e.rarity));
            total += baseW;
        }
        if (total <= 0) return null;

        int roll = Random.Range(0, total);
        foreach (var e in list)
        {
            int w = pool.WeightFor(e);
            w = Mathf.RoundToInt(w * CharacterEffectsManager.GetRarityWeightMultiplier(e.rarity)); // NEW
            if (w <= 0) continue;
            if (roll < w) return e;
            roll -= w;
        }
        return null;
    }

    public void MarkSold(Item item)
    {
        if (item == null) return;
        _soldNames.Add(item.itemName);
    }

    public Item[] GetVisibleItems()
    {
        EnsureSelectionForCurrentLevel();

        var result = new Item[itemsPerShop];
        for (int i = 0; i < itemsPerShop; i++)
        {
            var it = (i < _selection.Count) ? _selection[i] : null;
            result[i] = (it != null && !_soldNames.Contains(it.itemName)) ? it : null;
        }
        return result;
    }

    // Save / Load bridge
    public void SaveTo(SaveData data)
    {
        if (data == null) return;
        EnsureSelectionForCurrentLevel();
        data.shopLevelTag = _selectionLevel;
        data.shopItemNames.Clear();
        data.shopSoldItemNames.Clear();

        foreach (var it in _selection)
            data.shopItemNames.Add(it ? it.itemName : "");

        foreach (var n in _soldNames)
            data.shopSoldItemNames.Add(n);
    }

    public void LoadFrom(SaveData data)
    {
        _selection.Clear();
        _soldNames.Clear();

        if (data == null || pool == null)
        {
            _selectionLevel = Mathf.Max(1, GameGrid.level);
            PadSelectionWithNulls();
            return;
        }

        _selectionLevel = (data.shopLevelTag > 0) ? data.shopLevelTag : Mathf.Max(1, GameGrid.level);

        // Build a lookup from pool by name for resolving Items
        var byName = new Dictionary<string, Item>();
        if (pool != null && pool.entries != null)
        {
            foreach (var e in pool.entries)
            {
                if (e != null && e.item != null && !byName.ContainsKey(e.item.itemName))
                    byName.Add(e.item.itemName, e.item);
            }
        }

        // Repopulate selection
        if (data.shopItemNames != null && data.shopItemNames.Count > 0)
        {
            foreach (var name in data.shopItemNames)
            {
                if (!string.IsNullOrEmpty(name) && byName.TryGetValue(name, out var item))
                    _selection.Add(item);
                else
                    _selection.Add(null);
            }
        }
        while (_selection.Count < itemsPerShop) _selection.Add(null);

        // Sold flags
        if (data.shopSoldItemNames != null)
            foreach (var n in data.shopSoldItemNames)
                if (!string.IsNullOrEmpty(n)) _soldNames.Add(n);
    }
}

