using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class inventory : MonoBehaviour
{
    //[SerializeField] public ItemDatabase _itemDatabase;
    public ItemDatabase _itemDatabase;
    public itemEffects effects;
    public List<Item> inv;
    public inventorySlot[] slots; //this is where the image of the item will be put into (probably)
    //public GameObject inventoryItemPrefab;
    public Image itemimg;
    public Item item;

    public void Start()
    {
        inv = new List<Item> { null, null, null };
        itemInv();
        AddItem(1);
        AddItem(2);
        AddItem(3);

    }

    public void Update()
    {
        itemInv();
    }

    public void itemInv()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (ItemExists(0))
            {
                //get the id of item and executes its specific effect
                int id = inv[0].itemId;
                //Debug.Log to make sure the correct item was selected
                effects.executeEffect(id);
                //sets the list of this specific item to null
                inv[0] = null;
                //remove the image
                slots[0].removeItem();
            }
            else
            {
                Debug.Log("No item in slot");
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (ItemExists(1))
            {
                int id = inv[1].itemId;
                effects.executeEffect(id);
                inv[1] = null;
                slots[1].removeItem();
            }
            else
            {
                Debug.Log("No item in slot");
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (ItemExists(2))
            {
                int id = inv[2].itemId;
                effects.executeEffect(id);
                inv[2] = null;
                slots[2].removeItem();
            }
            else
            {
                Debug.Log("No item in slot");
            }
        }
    }

    public bool ItemExists(int slotNumber)
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

    public void AddItem(int itemId)
    {
        //In the future when we implement the store, make it so this function activates from pressing a button
        //Gets item from database based on its id
        Item itemAdded = _itemDatabase.GetItem(itemId);
        //Debug.Log(itemAdded);

        //Loop through inventory to find first empty slot
        for(int i=0; i < inv.Count; i++)
        {
            if (inv[i] == null)
            {
                inv[i] = itemAdded;
                slots[i].updateImage(itemAdded);
                Debug.Log("Spawned item with id" + (i+1));
                break;
            }
        }

    }

    public void RemoveItem(int i)
    {
        inv[i] = null;
    }
}