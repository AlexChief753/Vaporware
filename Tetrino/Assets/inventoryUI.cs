using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class inventoryUI : MonoBehaviour
{
    public List<Item> items;
    public Transform itemSlot;
    public GameObject itemSlotImage;

    public void UpdateInventory(Item items)
    {
        foreach (Transform item in itemSlot)
        {
            Destroy(item.gameObject);
        }

        foreach (Item item in items)
        {
            GameObject itemSpace = Instantiate(itemSlotImage, itemSlot);
            itemSpace.transform.Find("icon").GetComponent<Image>().sprite = item.icon;
        }
    }
}
