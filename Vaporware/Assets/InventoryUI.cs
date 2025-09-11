using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{

    public Image slot1;
    public Image slot2;
    public Image slot3;

    void Update()
    {
        // Keyboard shortcuts
        if (Input.GetKeyDown(KeyCode.Z)) TryUse(0);
        if (Input.GetKeyDown(KeyCode.X)) TryUse(1);
        if (Input.GetKeyDown(KeyCode.C)) TryUse(2);
    }

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

    public void OnClickSlot1() => TryUse(0);
    public void OnClickSlot2() => TryUse(1);
    public void OnClickSlot3() => TryUse(2);

    private void TryUse(int index)
    {
        InventoryManager.instance.UseItemAt(index);
    }

    private void SetSlot(Image slotImage, Item item)
    {
        // Set the circle's sprite to the item's sprite
        slotImage.sprite = item.itemSprite;
        // Make sure the Image is fully visible (alpha = 1)
        slotImage.color = Color.white;
        slotImage.rectTransform.sizeDelta = new Vector2(100, 100); // Make item sprite fit circle

        // Enable clicking only if an item exists in the slot
        var btn = slotImage.GetComponent<Button>();
        if (btn != null) btn.interactable = true;
    }

    private void ClearSlot(Image slotImage)
    {
        // Remove any sprite and set alpha to 0 (invisible)
        slotImage.sprite = null;
        slotImage.color = new Color(1, 1, 1, 0);

        // Disable clicking when empty
        var btn = slotImage.GetComponent<Button>();
        if (btn != null) btn.interactable = false;
    }
}
