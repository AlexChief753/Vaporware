using UnityEngine;
using UnityEngine.UI;

public class inventorySlot : MonoBehaviour
{
    public Image itemimg;
    public Item itemstore;

    public void updateImage(Item items)
    {
        itemstore = items;
        itemimg.sprite = itemstore.icon;
        itemimg.enabled = true;
        Debug.Log("Created image of item with id" + itemstore.itemId);
    }

    public void removeItem()
    {
        itemstore = null;
        itemimg.sprite = null;
        itemimg.enabled = false;
    }
}
