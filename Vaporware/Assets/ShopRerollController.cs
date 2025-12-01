using UnityEngine;
using UnityEngine.UI;
using TMPro;

[AddComponentMenu("Game/Shop/Shop Reroll Controller")]
public class ShopRerollController : MonoBehaviour
{
    [Header("Hook up your reroll button here")]
    public Button rerollButton;

    [Header("Cost")]
    public int rerollCost = 1000; //******************************************

    [Header("Pool (optional)")]
    [Tooltip("If left empty, will try to read from ShopService.pool. If that fails, this is required.")]
    public ItemPoolSO fallbackPool;

    private ItemShopManager shop;
    private ShopService svc;

    private void Awake()
    {
        shop = GetComponent<ItemShopManager>();
        svc = ShopService.FindOrCreate();

        if (rerollButton != null)
        {
            var label = rerollButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = $"Reroll ${rerollCost}";

            rerollButton.onClick.RemoveAllListeners();
            rerollButton.onClick.AddListener(OnClickReroll);
        }
    }

    private void OnClickReroll()
    {
        // Funds check
        if (GameGrid.currency < rerollCost)
        {
            Debug.Log("Reroll failed: not enough currency.");
            return;
        }

        // Deduct currency
        GameGrid.currency -= rerollCost;

        // Immediately refresh the currency label in the shop (same field used by ItemShopManager)
        if (shop != null && shop.currencyText != null)
            shop.currencyText.text = GameGrid.currency.ToString(); // mirrors UpdateCurrencyUI() logic. :contentReference[oaicite:2]{index=2}

        // Get a pool to roll from
        ItemPoolSO poolToUse = (svc != null && svc.pool != null) ? svc.pool : fallbackPool;
        if (poolToUse == null)
        {
            Debug.LogWarning("ShopRerollController: No ItemPoolSO available. Assign fallbackPool or ensure ShopService has a pool.");
            return;
        }

        // Roll a fresh set of 4 items (distinct within this roll)
        Item[] newItems = ItemPoolRoller.SampleDistinct(poolToUse, 4);

        // Update the internal shop state
        svc.ForceSetSelection(newItems);

        // Overwrite the shop's current selection (force all 4 to repopulate even if some were bought)
        if (shop != null)
        {
            // Ensure the array exists & has 4 slots (ItemShopManager.PopulateShop expects this). :contentReference[oaicite:3]{index=3}
            if (shop.availableItems == null || shop.availableItems.Length != 4)
                shop.availableItems = new Item[4];

            for (int i = 0; i < shop.availableItems.Length; i++)
                shop.availableItems[i] = (i < newItems.Length) ? newItems[i] : null;

            shop.OpenShop();
        }

        // Refresh level complete menu currency
        if (LevelManager.instance != null)
            LevelManager.instance.RefreshLevelCompleteUI();
    }
}

