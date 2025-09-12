using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    public int maxItems = 3;
    public List<Item> items = new List<Item>();

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
        if (items.Count >= maxItems)
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

        // TODO: replace this with the real effect later
        Debug.Log("Using item: " + item.itemName);

        // Remove item from inventory
        items.RemoveAt(index);

        // Refresh UI after use
        var ui = FindFirstObjectByType<InventoryUI>();
        if (ui != null) ui.RefreshSlots();
    }

}

