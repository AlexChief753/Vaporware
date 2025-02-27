using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class inventory : MonoBehaviour
{
    [SerializeField] private ItemDatabase _itemDatebase;
    //Change number to change how many inventory slots does the character have
    public List<Item> inv = new List<Item> {null,null,null};

    public ItemDatabase itemDatabase { get; private set; }
    public Item item { get; private set; }
    public inventoryUI ui;
    public itemEffects effects;

    public void itemInv()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (itemExists(1))
            {
                //get the id of item and executes its specific effect
                int id = item.itemId;
                effects.executeEffect(id);
                //removeItem();
            }
        }
        if (Input.GetKeyDown(KeyCode.X))
        {

        }
        if (Input.GetKeyDown(KeyCode.C))
        {

        }
    }

    public bool itemExists(int slotNumber)
    {
        if (inv[slotNumber] != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void addItem(int id)
    {
        //In the future when we implement the store, make it so this function activates from pressing a button
        Item itemAdded = itemDatabase.GetItem(id);
        //goes through each slot in inventory
        foreach (var slot in inv)
        {
            for (int i = 0; i < inv.Count; i++)
            {
                if (inv[i] != null)
                {
                    continue;
                }
                else
                {
                    inv[i] = itemAdded;
                    ui.UpdateInventory(itemAdded);
                    break;
                }

            }
        }

    }

    public void removeItem(int itemId)
    {
        Item itemRemove = itemDatabase.GetItem(itemId);
        inv.Remove(itemRemove);
    }
}