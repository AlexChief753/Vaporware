using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    // Assign these in the Inspector to your 3 circle Image objects
    public Image slot1;
    public Image slot2;
    public Image slot3;

    // Call this whenever the inventory changes
    public void RefreshSlots()
    {
        // Clear all slots first (hide or reset sprite)
        ClearSlot(slot1);
        ClearSlot(slot2);
        ClearSlot(slot3);

        // Go through each item in the inventory and display it
        for (int i = 0; i < InventoryManager.instance.items.Count; i++)
        {
            if (i == 0) { SetSlot(slot1, InventoryManager.instance.items[i]); }
            else if (i == 1) { SetSlot(slot2, InventoryManager.instance.items[i]); }
            else if (i == 2) { SetSlot(slot3, InventoryManager.instance.items[i]); }
        }
    }

    private void SetSlot(Image slotImage, Item item)
    {
        // Set the circle's sprite to the item's sprite
        slotImage.sprite = item.itemSprite;
        // Make sure the Image is fully visible (alpha = 1)
        slotImage.color = Color.white;
        slotImage.rectTransform.sizeDelta = new Vector2(100, 100); // Make item sprite fit circle
    }

    private void ClearSlot(Image slotImage)
    {
        // Remove any sprite and set alpha to 0 (invisible)
        slotImage.sprite = null;
        slotImage.color = new Color(1, 1, 1, 0);
    }
}

