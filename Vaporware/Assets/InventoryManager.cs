using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    public int maxItems = 3;
    public List<Item> items = new List<Item>();
    public List<Item> passiveItems = new List<Item>();
    public static float itemSpeedMod = 1;
    public static float scalingMult = 1;
    public static bool QuadDamActive = false;
    public static bool RecycleBinGarbage = false;
    public static int ResetRarities = 0;
    public static int GarbageDef = 0; // from bandage, helps block garbage from spawning


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Adds an item if there is room; returns true if successful
    public bool AddItem(Item newItem)
    {
        if (newItem.passive == true)
        {
            passiveItems.Add(newItem);
            Debug.Log("Passive item added: " + newItem.itemName);
            return true;
        }

        else if (items.Count >= maxItems)
        {
            Debug.Log("Inventory is full!");
            return false;
        }
        items.Add(newItem);
        Debug.Log("Item added: " + newItem.itemName);
        return true;
    }

    public void UseItemAt(int index)
    {
        if (index < 0 || index >= items.Count)
        {
            Debug.Log("No item in that slot.");
            return;
        }

        Item item = items[index];

        Debug.Log("Using item: " + item.itemName);

        // Force Sequence item (Files)
        if (item.itemName == "Files")
        {
            // Find the currently falling Tetromino
            Tetromino current = FindFirstObjectByType<Tetromino>();
            if (current != null)
            {
                int type = current.pieceIndex;
                Spawner spawner = FindFirstObjectByType<Spawner>();
                if (spawner != null)
                {
                    spawner.ForceSequence(type, 3); // Number of times to force the same piece to spawn
                    Debug.Log($"Files used: tetromino will repeat.");
                }
            }
        }

        // Remove 3 rows from top item (Scissors)
        if (item.itemName == "Scissors")
        {
            int cleared = GameGrid.ClearTopNOccupiedRows(3);
            Debug.Log($"Scissors used: cleared {cleared} top occupied row(s).");
        }

        // Remove 3 rows from bottom (Shredder)
        if (item.itemName == "Shredder")
        {
            int cleared = GameGrid.ClearBottomNRows(3);
            Debug.Log($"Shredder used: removed {cleared} bottom rows.");
        }

        if (item.itemName == "Investment")
        {
            QuadDamActive = true;
        }

        if (item.itemName == "Canned Combo")
        {
            GameGrid.comboProtect = true;
            GameGrid.comboCount += 5;
        }

        if (item.itemName == "Olive Branch")
        {
            var bossMan = FindFirstObjectByType<BossManager>();
            bossMan.BossDeactivate();
        }

        if (item.itemName == "Call Off")
            if ((GameGrid.level % 4) != 0)
                GameGrid.UpdateLevelForced();

        if (item.itemName == "Recycling Bin")
        {
            GameGrid.currency += 250;
            RecycleBinGarbage = true;
        }

        if (item.itemName == "Bag O' Four: I")
        {
            var player = FindFirstObjectByType<Player>();
            player.playerBag.Add(3);
            Spawner spawner = FindFirstObjectByType<Spawner>();
            if (spawner != null)
            {
                spawner.ForceSequence(3, 1);
            }
        }

        if (item.itemName == "Bag O' Four: O")
        {
            var player = FindFirstObjectByType<Player>();
            player.playerBag.Add(1);
            Spawner spawner = FindFirstObjectByType<Spawner>();
            if (spawner != null)
            {
                spawner.ForceSequence(1, 1);
            }
        }

        if (item.itemName == "Bag O' Four: Z")
        {
            var player = FindFirstObjectByType<Player>();
            player.playerBag.Add(6);
            Spawner spawner = FindFirstObjectByType<Spawner>();
            if (spawner != null)
            {
                spawner.ForceSequence(6, 1);
            }
        }

        if (item.itemName == "Bag O' Four: J")
        {
            var player = FindFirstObjectByType<Player>();
            player.playerBag.Add(4);
            Spawner spawner = FindFirstObjectByType<Spawner>();
            if (spawner != null)
            {
                spawner.ForceSequence(4, 1);
            }
        }

        if (item.itemName == "Bag O' Four: S")
        {
            var player = FindFirstObjectByType<Player>();
            player.playerBag.Add(5);
            Spawner spawner = FindFirstObjectByType<Spawner>();
            if (spawner != null)
            {
                spawner.ForceSequence(5, 1);
            }
        }

        if (item.itemName == "Bag O' Four: T")
        {
            var player = FindFirstObjectByType<Player>();
            player.playerBag.Add(0);
            Spawner spawner = FindFirstObjectByType<Spawner>();
            if (spawner != null)
            {
                spawner.ForceSequence(0, 1);
            }
        }

        if (item.itemName == "Bag O' Four: L")
        {
            var player = FindFirstObjectByType<Player>();
            player.playerBag.Add(2);
            Spawner spawner = FindFirstObjectByType<Spawner>();
            if (spawner != null)
            {
                spawner.ForceSequence(2, 1);
            }
        }

        if (item.itemName == "Rabbit's Foot")
        {
            var itemPool = FindFirstObjectByType<ItemPoolSO>();
            itemPool.commonWeight -= 10;
            itemPool.uncommonWeight += 5;
            itemPool.rareWeight += 10;
            itemPool.epicWeight += 15;
            itemPool.legendaryWeight += 15;
            ResetRarities++;
        }

        if (item.itemName == "Coffee")
        {
            Debug.Log($"Current value of lastLineCleared: " + GameGrid.lastLineCleared);
            var levelMan = FindFirstObjectByType<LevelManager>();
            Debug.Log($"Current value of currentTime: " + levelMan.GetRemainingTime());
        }

        // Remove item from inventory
        items.RemoveAt(index);

        // Refresh UI after use
        var ui = FindFirstObjectByType<InventoryUI>();
        if (ui != null) ui.RefreshSlots();
    }

    //run at level start to activate items that should be active immediately
    public void PassiveInit()
    {
        var levelMan = FindFirstObjectByType<LevelManager>();
        itemSpeedMod = 1;               //reset any values modified to default before applying effects
        GameGrid.comboReset = true;     //in order to prevent reapplying effects
        GarbageDef = 0;
        GameGrid.lastLineCleared = levelMan.GetRemainingTime();

        for (int i = 0; i < passiveItems.Count; i++)
        {
            if (passiveItems[i].itemName == "Ice Pack")
                itemSpeedMod = itemSpeedMod * (float)0.5;

            if (passiveItems[i].itemName == "CD Player")
                GameGrid.comboReset = false;

            if (passiveItems[i].itemName == "Bandage")
                GarbageDef++;
        }

        if (ResetRarities < 0)
            for (int i = 0; i < ResetRarities; i++)
            {
                var itemPool = FindFirstObjectByType<ItemPoolSO>();
                itemPool.commonWeight += 10;
                itemPool.uncommonWeight -= 5;
                itemPool.rareWeight -= 10;
                itemPool.epicWeight -= 15;
                itemPool.legendaryWeight -= 15;
            }
        ResetRarities = 0;
        return;
    }

    public void PassiveEndRound()
    {
        for (int i = 0; i < passiveItems.Count; i++)
        {
            if (passiveItems[i].itemName == "Time is Money")
            {
                var levelMan = FindFirstObjectByType<LevelManager>();
                GameGrid.currency += (int)Mathf.Round((float)(levelMan.GetRemainingTime() * 0.2));
            }

            if (passiveItems[i].itemName == "Pay Raise")
                GameGrid.currency += 50;

            if (passiveItems[i].itemName == "Trash Bag")
                GameGrid.currency += 75;
        }
        return;
    }
}

