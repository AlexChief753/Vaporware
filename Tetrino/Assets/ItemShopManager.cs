using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemShopManager : MonoBehaviour
{
    // The panel that holds the shop UI (set this in the Inspector)
    public GameObject itemShopPanel;
    // A prefab for each item button (should have an Image and a TextMeshProUGUI child)
    public Button itemButtonPrefab;
    // Parent container (e.g., a vertical layout group) to hold the buttons
    public Transform itemsParent;
    // Back button to return to the previous menu
    public Button backButton;

    // Array of available items in the shop (set these in the Inspector)
    public Item[] availableItems;

    void Start()
    {
        // Make sure the shop is hidden at first.
        itemShopPanel.SetActive(false);
        backButton.onClick.AddListener(OnBack);
    }

    // Call this to show the shop
    public void OpenShop()
    {
        itemShopPanel.SetActive(true);
        PopulateShop();
    }

    // Hide the shop panel
    public void CloseShop()
    {
        itemShopPanel.SetActive(false);
    }

    // Instantiate a button for each available item
    void PopulateShop()
    {
        // Clear any existing buttons
        foreach (Transform child in itemsParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Item item in availableItems)
        {
            Button newButton = Instantiate(itemButtonPrefab, itemsParent);
            // Set the button’s image to the item’s sprite
            Image img = newButton.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = item.itemSprite;
            }
            // Optionally, set text on the button
            TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = item.itemName;
            }
            // When the button is clicked, attempt to purchase the item
            newButton.onClick.AddListener(() => PurchaseItem(item));
        }
    }

    // Attempt to add the item to the inventory
    void PurchaseItem(Item item)
    {
        if (InventoryManager.instance.AddItem(item))
        {
            Debug.Log("Purchased: " + item.itemName);
            // You could remove or disable the button if the item is meant to be one-time only.
            FindFirstObjectByType<InventoryUI>().RefreshSlots();
        }
        else
        {
            Debug.Log("Cannot purchase, inventory full!");
        }
    }

    // Called when the Back button is pressed to close the shop
    void OnBack()
    {
        CloseShop();
        if (LevelManager.instance != null)
        {
            LevelManager.instance.levelCompleteMenu.SetActive(true);
        }
    }
}

