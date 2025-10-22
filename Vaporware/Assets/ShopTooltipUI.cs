using UnityEngine;
using TMPro;

public class ShopTooltipUI : MonoBehaviour
{
    [Header("Wiring")]
    public GameObject root;            // The panel to show/hide
    public TextMeshProUGUI titleText;  // Item name
    public TextMeshProUGUI bodyText;   // Item description

    void Awake()
    {
        if (root != null) root.SetActive(false);
    }

    public void Show(string title, string body)
    {
        if (!root) return;
        if (titleText) titleText.text = title ?? "";
        if (bodyText) bodyText.text = body ?? "";
        root.SetActive(true);
    }

    public void Hide()
    {
        if (root) root.SetActive(false);
    }
}

