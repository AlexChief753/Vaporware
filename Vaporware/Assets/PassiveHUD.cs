using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Inventory/Passive HUD")]
public class PassiveHUD : MonoBehaviour
{
    [Header("Grid Root (has GridLayoutGroup)")]
    public Transform gridParent;

    [Header("Prefab for each passive icon")]
    public PassiveItemIcon iconPrefab;

    // Cached pool of spawned icons by Item
    private readonly Dictionary<Item, PassiveItemIcon> _icons = new Dictionary<Item, PassiveItemIcon>();

    // Simple signature to detect changes without touching existing code
    private int _lastSignature = -1;

    void Start()
    {
        // If the user forgot to add a GridLayoutGroup, add a nice default
        if (gridParent && !gridParent.GetComponent<GridLayoutGroup>())
        {
            var g = gridParent.gameObject.AddComponent<GridLayoutGroup>();
            g.cellSize = new Vector2(38, 38);
            g.spacing = new Vector2(4, 4);
            g.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            g.constraintCount = 8; // small, wide row under your three active slots
            g.childAlignment = TextAnchor.UpperLeft;
        }

        RefreshNow();
    }

    void LateUpdate()
    {
        // Poll for changes—keeps this feature in new scripts only
        var sig = ComputeSignature();
        if (sig != _lastSignature)
        {
            RefreshNow();
            _lastSignature = sig;
        }
    }

    public void RefreshNow()
    {
        if (InventoryManager.instance == null || gridParent == null || iconPrefab == null) return;

        // 1) Count duplicates
        var counts = new Dictionary<Item, int>();
        foreach (var it in InventoryManager.instance.passiveItems)
        {
            if (it == null) continue;
            counts.TryGetValue(it, out int c);
            counts[it] = c + 1;
        }

        // 2) Mark existing icons we’ll keep this frame
        var toKeep = new HashSet<Item>(counts.Keys);

        // 3) Remove icons for items no longer present
        var removeList = new List<Item>();
        foreach (var kvp in _icons)
            if (!toKeep.Contains(kvp.Key))
                removeList.Add(kvp.Key);
        foreach (var key in removeList)
        {
            if (_icons[key] != null) Destroy(_icons[key].gameObject);
            _icons.Remove(key);
        }

        // 4) Create or update icons for current items
        foreach (var kv in counts)
        {
            var item = kv.Key;
            var count = kv.Value;

            PassiveItemIcon icon;
            if (!_icons.TryGetValue(item, out icon) || icon == null)
            {
                icon = Instantiate(iconPrefab, gridParent);
                _icons[item] = icon;
            }

            icon.name = $"PassiveIcon_{item.itemName}";
            icon.Set(item.itemSprite, count);
        }

        // 5) Optional: sort icons by item name for stable layout
        SortIconsByName();
    }

    private void SortIconsByName()
    {
        // Reorder children by name (stable visual order)
        var list = new List<Transform>();
        foreach (Transform t in gridParent) list.Add(t);
        list.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
        for (int i = 0; i < list.Count; i++)
            list[i].SetSiblingIndex(i);
    }

    // Build a quick hash of the passive inventory contents
    private int ComputeSignature()
    {
        if (InventoryManager.instance == null) return 0;
        unchecked
        {
            int h = 17;
            var list = InventoryManager.instance.passiveItems;
            h = h * 31 + list.Count;
            for (int i = 0; i < list.Count; i++)
            {
                var it = list[i];
                h = h * 31 + (it ? it.GetInstanceID() : 0);
            }
            return h;
        }
    }
}

