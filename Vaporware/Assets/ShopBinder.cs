using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Game/Shop/Shop Binder")]
[RequireComponent(typeof(ItemShopManager))]
public class ShopBinder : MonoBehaviour
{
    private ItemShopManager shop;
    private ShopService svc;

    private bool lastPanelActive = false;

    [Header("Fallback for ItemPoolSO")]
    public ItemPoolSO fallbackPool;

    void Awake()
    {
        shop = GetComponent<ItemShopManager>();
        svc = ShopService.FindOrCreate();

        // If the service came up without a pool, inject the fallback
        if (svc != null && svc.pool == null && fallbackPool != null)
            svc.pool = fallbackPool;

        // On boot, prime availableItems so when LevelManager opens the shop,
        // ItemShopManager.PopulateShop() sees the current selection.
        SyncAvailableFromService();
    }

    void Update()
    {
        if (shop == null) return;

        bool nowActive = shop.itemShopPanel != null && shop.itemShopPanel.activeInHierarchy;

        // When panel *opens*, wait a frame and attach click hooks to each button
        if (nowActive && !lastPanelActive)
        {
            // NEW: refresh the shop array from the service right when opening
            SyncAvailableFromService();
            StartCoroutine(AfterOpenHook());
        }

        // When panel is closed, keep availableItems synchronized,
        // and let the service roll a new set automatically once the level changes.
        if (!nowActive)
        {
            SyncAvailableFromService();
        }

        lastPanelActive = nowActive;
    }

    private void SyncAvailableFromService()
    {
        if (svc == null) svc = ShopService.FindOrCreate();
        if (svc == null) return;

        var visible = svc.GetVisibleItems();
        // Guarantee 4 slots for existing PopulateShop()
        if (shop.availableItems == null || shop.availableItems.Length != visible.Length)
            shop.availableItems = new Item[visible.Length];

        for (int i = 0; i < visible.Length; i++)
            shop.availableItems[i] = visible[i];

    }

    private IEnumerator AfterOpenHook()
    {
        // Let ItemShopManager.PopulateShop() run first
        yield return null;


    }
}

