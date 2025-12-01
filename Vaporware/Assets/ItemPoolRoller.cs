using System.Collections.Generic;
using UnityEngine;

public static class ItemPoolRoller
{

    public static Item[] SampleDistinct(ItemPoolSO pool, int count)
    {
        var result = new Item[count];
        if (pool == null || pool.entries == null || pool.entries.Count == 0)
            return result;

        // Build a working list of eligible entries
        var candidates = new List<ItemPoolSO.Entry>();
        foreach (var e in pool.entries)
        {
            if (e != null && e.item != null && e.enabled)
                candidates.Add(e);
        }

        for (int i = 0; i < count; i++)
        {
            if (candidates.Count == 0) break;

            // Weighted pick
            int total = 0;
            for (int k = 0; k < candidates.Count; k++)
                total += Mathf.Max(0, pool.WeightFor(candidates[k]));

            if (total <= 0) break;

            int roll = Random.Range(0, total);
            int running = 0;
            int pickIndex = -1;

            for (int k = 0; k < candidates.Count; k++)
            {
                running += Mathf.Max(0, pool.WeightFor(candidates[k]));
                if (roll < running)
                {
                    pickIndex = k;
                    break;
                }
            }

            if (pickIndex >= 0)
            {
                result[i] = candidates[pickIndex].item;
                candidates.RemoveAt(pickIndex); // ensure distinct results in this sample
            }
            else break;
        }

        return result;
    }
}

