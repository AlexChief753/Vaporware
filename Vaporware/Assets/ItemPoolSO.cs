using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemPool", menuName = "Game/Shop/Item Pool")]
public class ItemPoolSO : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public Item item;
        public ItemRarity rarity = ItemRarity.Common;
        [Tooltip("If > 0, overrides rarity weight")]
        public int weightOverride = 0;
        public bool enabled = true;
    }

    [Header("Pool")]
    public List<Entry> entries = new();

    [Header("Base rarity weights (used when weightOverride <= 0)")]
    public int commonWeight = 60;
    public int uncommonWeight = 25;
    public int rareWeight = 10;
    public int epicWeight = 4;
    public int legendaryWeight = 1;

    public int WeightFor(Entry e)
    {
        if (e == null || e.item == null || !e.enabled) return 0;
        if (e.weightOverride > 0) return e.weightOverride;
        return e.rarity switch
        {
            ItemRarity.Common => Mathf.Max(0, commonWeight),
            ItemRarity.Uncommon => Mathf.Max(0, uncommonWeight),
            ItemRarity.Rare => Mathf.Max(0, rareWeight),
            ItemRarity.Epic => Mathf.Max(0, epicWeight),
            ItemRarity.Legendary => Mathf.Max(0, legendaryWeight),
            _ => 0
        };
    }
}

