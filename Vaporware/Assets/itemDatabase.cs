using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public List<Item> itemDB = new List<Item>();

    public Item GetItem(int id)
    {
        Item item = itemDB.Find(item => item.itemId == id);
        return item;
    }
}
