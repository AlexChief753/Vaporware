using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{

    public Image slot1;
    public Image slot2;
    public Image slot3;

    [Header("Lock control")]
    public CanvasGroup rootGroup;

    // Remember each slot’s original Navigation so can restore it
    private readonly Dictionary<Button, Navigation> _savedNav = new Dictionary<Button, Navigation>();

    public void SetMenuLock(bool locked)
    {
        // Globally block all interactions/raycasts under the inventory
        if (rootGroup != null)
        {
            rootGroup.interactable = !locked;
            rootGroup.blocksRaycasts = !locked;
        }

        // Remove each slot Button from the navigation graph
        foreach (var img in new[] { slot1, slot2, slot3 })
        {
            if (!img) continue;
            var btn = img.GetComponent<Button>();
            if (!btn) continue;

            if (locked)
            {
                if (!_savedNav.ContainsKey(btn)) _savedNav[btn] = btn.navigation;
                var nav = btn.navigation;
                nav.mode = Navigation.Mode.None;
                btn.navigation = nav;

                // Ensure nothing can select/click it even if SetSlot runs later
                btn.interactable = false;
            }
            else
            {
                // Restore nav and interactable according to whether a sprite is present
                if (_savedNav.TryGetValue(btn, out var original))
                    btn.navigation = original;

                btn.interactable = (img.sprite != null);
            }
        }
    }



    void Update()
    {
        // Don’t accept inventory input when the Level Complete menu is up
        //if (LevelManager.instance != null &&
        //    LevelManager.instance.levelCompleteMenu != null &&
         //   LevelManager.instance.levelCompleteMenu.activeInHierarchy)
         //   return;

        if (LevelManager.instance && LevelManager.instance.levelCompleteMenu &&
            LevelManager.instance.levelCompleteMenu.activeInHierarchy) return;

            // Keyboard shortcuts
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetButtonDown("UseSlot1")) 
            TryUse(0);
        if (Input.GetKeyDown(KeyCode.X) || Input.GetButtonDown("UseSlot2")) 
            TryUse(1);
        if (Input.GetKeyDown(KeyCode.C) || Input.GetButtonDown("UseSlot3")) 
            TryUse(2);
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
        //if (btn != null) btn.interactable = true;
        if (btn != null)
        {
            // If locked, stay non-interactable no matter what
            bool locked = rootGroup != null && !rootGroup.interactable;
            btn.interactable = !locked;   
        }
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
