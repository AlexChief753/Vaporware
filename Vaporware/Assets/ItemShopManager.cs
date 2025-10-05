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

        for (int i = 0; i < availableItems.Length; i++)
        {
            var item = availableItems[i];
            if (item == null) continue;

            Button newButton = Instantiate(itemButtonPrefab, itemsParent);

            var buttonImage = newButton.GetComponent<Image>();
            if (buttonImage != null) buttonImage.sprite = item.itemSprite;

            var nameText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null) nameText.text = item.itemName;

            var priceText = newButton.transform.Find("PriceBadge/PriceText")?.GetComponent<TextMeshProUGUI>();
            if (priceText != null) priceText.text = $"${item.price}";

            var priceBadge = newButton.transform.Find("PriceBadge")?.GetComponent<Image>();
            if (priceBadge != null) priceBadge.enabled = true;

            int capturedIndex = i;
            newButton.onClick.RemoveAllListeners();
            newButton.onClick.AddListener(() =>
            {
                // Only remove from shop when purchase actually succeeds
                if (PurchaseItem(item))
                {
                    // mark sold for this round
                    ShopService.FindOrCreate().MarkSold(item);

                    // make sure re-entering shop this level doesn't bring it back
                    availableItems[capturedIndex] = null;

                    // hide this specific button
                    newButton.gameObject.SetActive(false);
                }
                else
                {
                    // we could have feedback for failure (flash price red, SFX, etc.)
                }
            });
        }
    }

    bool PurchaseItem(Item item)
    {
        if (item == null) return false;

        if (GameGrid.currency < item.price)
        {
            Debug.Log("Cannot purchase, not enough currency!");
            return false;
        }

        // Try to add to inventory
        if (!InventoryManager.instance.AddItem(item))
        {
            Debug.Log("Cannot purchase, inventory full!");
            return false;
        }

        // Deduct and refresh
        GameGrid.currency -= item.price;
        var ui = FindFirstObjectByType<InventoryUI>();
        if (ui) ui.RefreshSlots();
        UpdateCurrencyUI();

        Debug.Log($"Purchased: {item.itemName} for {item.price}");

        return true;
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

        // If selection was lost, reselect something
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


