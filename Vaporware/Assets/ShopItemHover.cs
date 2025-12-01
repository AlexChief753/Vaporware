using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopItemHover : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler
{
    public Item item;                 // Assigned at runtime
    public ShopTooltipUI tooltip;     // Assigned at runtime

    // Optional: keep tooltip visible while this is selected even if mouse leaves
    private bool _isSelected;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!tooltip || !item) return;
        tooltip.Show(item.itemName, item.description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!tooltip || _isSelected) return; // keep visible if selected by controller
        tooltip.Hide();
    }

    public void OnSelect(BaseEventData eventData)
    {
        _isSelected = true;
        if (!tooltip || !item) return;
        tooltip.Show(item.itemName, item.description);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        _isSelected = false;
        if (!tooltip) return;
        tooltip.Hide();
    }
}

