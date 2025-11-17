using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PassiveItemIcon : MonoBehaviour
{
    [Header("UI")]
    public Image iconImage;
    public TextMeshProUGUI countText;

    /// <summary>Sets the sprite and the "x N" count.</summary>
    public void Set(Sprite sprite, int count)
    {
        if (iconImage) iconImage.sprite = sprite;
        if (countText) countText.text = (count > 1 ? $"x {count}" : "x 1");
        // ensure visible
        if (iconImage) iconImage.color = Color.white;
        if (countText) countText.enabled = true;
    }
}

