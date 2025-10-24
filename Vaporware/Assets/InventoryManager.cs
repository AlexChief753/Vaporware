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

        // Remove item from inventory
        items.RemoveAt(index);

        // Refresh UI after use
        var ui = FindFirstObjectByType<InventoryUI>();
        if (ui != null) ui.RefreshSlots();
    }

    //run at level start to activate items that should be active immediately
    public void PassiveInit()
    {
        itemSpeedMod = 1;               //reset any values modified to default before applying effects
        GameGrid.comboReset = true;     //in order to prevent reapplying effects

        for (int i = 0; i < passiveItems.Count; i++)
        {
            if (passiveItems[i].itemName == "Ice Pack")
                itemSpeedMod = itemSpeedMod * (float) 0.5;

            if (passiveItems[i].itemName == "CD Player")
                GameGrid.comboReset = false;

        }
        return;
    }
}

