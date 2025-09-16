using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemShopManager : MonoBehaviour
{
    public GameObject itemShopPanel;
    public Button itemButtonPrefab;
    public Transform itemsParent;
    public Button backButton;

    public TextMeshProUGUI currencyText;

    // reference Item ScriptableObject assets
    public Item[] availableItems;

    private GameObject lastSelectedShopGO;

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

        //arm selection for controller nav
        StartCoroutine(ArmShopSelection());

        Button first = itemsParent.GetComponentInChildren<Button>();
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject((first != null ? first.gameObject : backButton.gameObject));
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

            // Name
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
        {
            LevelManager.instance.levelCompleteMenu.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(LevelManager.instance.continueButton.gameObject);
        }

    }

    void UpdateCurrencyUI()
    {
        if (currencyText != null)
            currencyText.text = "Currency: " + GameGrid.currency.ToString();
    }

    private void SelectFirstShopButton()
    {
        // Try the first item button; if none, fall back to Back
        Button firstItem = itemsParent ? itemsParent.GetComponentInChildren<Button>(true) : null;
        GameObject target = firstItem ? firstItem.gameObject : (backButton ? backButton.gameObject : null);
        if (!target) return;

        var es = EventSystem.current;
        if (!es) return;

        es.SetSelectedGameObject(null);
        es.SetSelectedGameObject(target);
        lastSelectedShopGO = target;
    }

    // Arm selection one frame later & after Submit is released
    private System.Collections.IEnumerator ArmShopSelection()
    {
        // Wait a frame so the panel is active and layout is done,
        // and buttons created by PopulateShop() actually exist.
        yield return null;

        // Drain any held A/Submit so we don't instantly click
        while (Input.GetButton("Submit")) yield return null;

        SelectFirstShopButton();
    }

    void Update()
    {
        if (!itemShopPanel || !itemShopPanel.activeInHierarchy) return;
        var es = EventSystem.current;
        if (!es) return;

        // If selection was lost (e.g., mouse clicked a non-selectable), reselect something
        if (es.currentSelectedGameObject == null || !es.currentSelectedGameObject.activeInHierarchy)
        {
            // Try to reselect the last selected shop GO first
            if (lastSelectedShopGO && lastSelectedShopGO.activeInHierarchy)
            {
                es.SetSelectedGameObject(lastSelectedShopGO);
            }
            else
            {
                SelectFirstShopButton();
            }
        }
    }

}


