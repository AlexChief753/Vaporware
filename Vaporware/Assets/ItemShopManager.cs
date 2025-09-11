using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemShopManager : MonoBehaviour
{
    public GameObject itemShopPanel;
    public Button itemButtonPrefab;
    public Transform itemsParent;
    public Button backButton;

    public TextMeshProUGUI currencyText;

    // Now this should reference Item ScriptableObject assets
    public Item[] availableItems;

    void Start()
    {
        itemShopPanel.SetActive(false);
        backButton.onClick.AddListener(OnBack);
    }

    public void OpenShop()
    {
        itemShopPanel.SetActive(true);
        UpdateCurrencyUI();
        PopulateShop();
    }

    public void CloseShop()
    {
        itemShopPanel.SetActive(false);
    }

    void PopulateShop()
    {
        // Clear existing
        foreach (Transform child in itemsParent)
            Destroy(child.gameObject);

        foreach (Item item in availableItems)
        {
            if (item == null) continue;

            Button newButton = Instantiate(itemButtonPrefab, itemsParent);

            // Icon (use Button's Image or a child "Icon")
            var buttonImage = newButton.GetComponent<Image>();
            if (buttonImage != null)
                buttonImage.sprite = item.itemSprite;

            // Optional: if you have a child named "Icon" instead
            // var icon = newButton.transform.Find("Icon")?.GetComponent<Image>();
            // if (icon) icon.sprite = item.itemSprite;

            // Name (if you keep showing it)
            var nameText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = item.itemName;

            // Price (child "PriceText") & badge visibility
            var priceText = newButton.transform.Find("PriceBadge/PriceText")?.GetComponent<TextMeshProUGUI>();
            if (priceText != null)
                priceText.text = $"${item.price}";

            var priceBadge = newButton.transform.Find("PriceBadge")?.GetComponent<Image>();
            if (priceBadge != null)
                priceBadge.enabled = true; // ensure the rectangle shows

            newButton.onClick.RemoveAllListeners();
            newButton.onClick.AddListener(() => PurchaseItem(item));
        }
    }

    void PurchaseItem(Item item)
    {
        if (item == null) return;

        // Use the item's own price instead of a fixed 100
        if (GameGrid.currency < item.price)
        {
            Debug.Log("Cannot purchase, not enough currency!");
            // TODO: flash price red / play SFX
            return;
        }

        // Try to add to inventory
        if (!InventoryManager.instance.AddItem(item))
        {
            Debug.Log("Cannot purchase, inventory full!");
            return;
        }

        // Deduct and refresh
        GameGrid.currency -= item.price;
        var ui = FindFirstObjectByType<InventoryUI>();
        if (ui) ui.RefreshSlots();
        UpdateCurrencyUI();

        Debug.Log($"Purchased: {item.itemName} for {item.price}");
    }

    void OnBack()
    {
        CloseShop();
        if (LevelManager.instance != null)
            LevelManager.instance.levelCompleteMenu.SetActive(true);
    }

    void UpdateCurrencyUI()
    {
        if (currencyText != null)
            currencyText.text = "Currency: " + GameGrid.currency.ToString();
    }
}


