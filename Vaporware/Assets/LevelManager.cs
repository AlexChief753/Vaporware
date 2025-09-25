using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public TextMeshProUGUI timerText;
    public GameObject levelCompleteMenu;
    public TextMeshProUGUI levelCompleteText;

    private float levelTime = 300f; // 5 minutes per level is 300f
    private int scoreRequirement = 1000;
    private float currentTime;
    private bool levelPaused = false;

    public Button continueButton;
    public Button itemShopButton;
    public Button saveButton;
    public Button quitButton;

    [Header("Pause Menu")]
    public GameObject pauseMenu;
    public Button pauseResumeButton; // first button to focus
    public Button pauseSettingsButton;      // placeholder
    public Button pauseMainMenuButton; // placeholder
    public Button pauseExitButton;

    public CanvasController textCanvasController;

    private bool isPaused = false;
    bool inputArmed = false;        // gate to ignore first Submit press

    [SerializeField] private Button firstMenuButton;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        else if (instance != this)
        {
            Destroy(gameObject); // prevent duplicates calling different flags
            return;
        }

        // Initialize timer here so it's ready before Spawner.Start()
        currentTime = levelTime;
    }

    void Start()
    {
        //currentTime = levelTime;
        UpdateTimerUI();
        levelCompleteMenu.SetActive(false);

        // Hide pause menu and wire buttons
        if (pauseMenu != null) pauseMenu.SetActive(false);

        if (pauseResumeButton != null)
            pauseResumeButton.onClick.AddListener(ResumeGame);

        if (pauseSettingsButton != null)
            pauseSettingsButton.onClick.AddListener(() => { Debug.Log("Settings (placeholder)"); });

        if (saveButton != null) saveButton.onClick.AddListener(SaveAtLevelComplete);

        if (pauseMainMenuButton != null)
            pauseMainMenuButton.onClick.AddListener(() => { Debug.Log("Main Menu (placeholder)"); });

        if (pauseExitButton != null)
            pauseExitButton.onClick.AddListener(QuitGame);

        // If launching via Load Game, re-open Level Complete menu in paused state
        if (GameSession.startMode == StartMode.LoadGame && GameSession.pendingSaveData != null)
        {
            ApplyLoadedDataAndShowLevelComplete(GameSession.pendingSaveData);
        }
    }

    void Update()
    {
        // Toggle Pause
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Pause"))
        {
            if (!isPaused)
            {
                if (CanOpenPauseMenu())
                    PauseGame();
            }
            else
            {
                // If already paused, Escape resumes
                ResumeGame();
            }
        }

        if (!levelPaused)
        {
            if (currentTime > 0f)
            {
                currentTime -= Time.deltaTime;
                if (currentTime < 0f) currentTime = 0f; // clamp so the clock stops at 00:00
            }

            UpdateTimerUI();
            // No level complete here — timeout is handled as Game Over via GameGrid.IsGameOver()
        }
    }

    public void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void CompleteLevel()
    {
        levelPaused = true;
        Time.timeScale = 0; // Pause the game
        levelCompleteMenu.SetActive(true);
        levelCompleteText.text = "Level " + GameGrid.level + " Complete!";

        // ensure inventory slots become non-interactable now
        var inv = FindFirstObjectByType<InventoryUI>();
        if (inv != null)
        {
            inv.RefreshSlots();   // draw items first
            inv.SetMenuLock(true); // then hard-lock them
        }

        inputArmed = false;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
        StartCoroutine(ArmMenuSelection());
        // Force controller focus to the first button
        var es = EventSystem.current;
        if (es != null && firstMenuButton != null)
        {
            es.SetSelectedGameObject(null);
            es.SetSelectedGameObject(firstMenuButton.gameObject);
        }
    }

    System.Collections.IEnumerator ArmMenuSelection()
    {
        // wait one frame so the panel is active and any gameplay Submit press is from the past frame
        yield return null;

        // drain any held Submit/A so we don't immediately click
        while (Input.GetButton("Submit")) yield return null;

        // now pick the first button for navigation
        if (continueButton != null)
            EventSystem.current.SetSelectedGameObject(continueButton.gameObject);

        inputArmed = true;
    }

    public void ContinueToNextLevel()
    {
        if (!levelPaused || !inputArmed) return;
        // Prefer a robust guard that matches the actual menu state:
        if (!levelCompleteMenu || !levelCompleteMenu.activeInHierarchy) return;

        // Unlock inventory befor hiding the menu
        var inv = FindFirstObjectByType<InventoryUI>();
        if (inv != null) inv.SetMenuLock(false);

        // if (Time.timeScale > 0f) return;

        // Proceed
        levelPaused = false;                // internal bookkeeping
        Time.timeScale = 1;
        GameGrid.level++;
        currentTime = levelTime;
        GameGrid.levelScore = 0;

        var inventoryManager = FindFirstObjectByType<InventoryManager>();
        inventoryManager.PassiveInit();  //hacky, but works?

        Tetromino.UpdateGlobalSpeed();
        GameGrid.levelUpTriggered = false;

        var activeTetromino = FindFirstObjectByType<Tetromino>();
        if (activeTetromino) Destroy(activeTetromino.gameObject);

        GameGrid.ClearGrid();

        var spawner = FindFirstObjectByType<Spawner>();
        if (spawner != null)
        {
            spawner.UpdateUI();
            spawner.SpawnTetromino();
        }
        else
        {
            Debug.LogError("Spawner not found in the scene!");
        }

        levelCompleteMenu.SetActive(false);

        // re-enable inventory buttons for gameplay
        //var ui = FindFirstObjectByType<InventoryUI>();
        //if (ui) ui.RefreshSlots();
    }


    public void OpenItemShop()
    {
        // Hide the level complete menu.
        levelCompleteMenu.SetActive(false);
        // Find the ItemShopManager and open the shop.
        ItemShopManager shopManager = FindFirstObjectByType<ItemShopManager>();
        if (shopManager != null)
        {
            shopManager.OpenShop();
        }
    }


    public void QuitGame()
    {
        Application.Quit();
    }

    public static void ModifyLevelParameters(float newTime, int newScore)
    {
        instance.levelTime = newTime;
        instance.scoreRequirement = newScore;
    }

    public float GetRemainingTime()
    {
        return currentTime;
    }

    private bool CanOpenPauseMenu()
    {
        // Not while a level-complete menu is shown
        if (levelCompleteMenu != null && levelCompleteMenu.activeInHierarchy) return false;

        // Not while the item shop is open
        var shop = FindFirstObjectByType<ItemShopManager>();
        if (shop != null && shop.itemShopPanel != null && shop.itemShopPanel.activeInHierarchy) return false;

        // Not while the Game Over UI is up
        var spawner = FindFirstObjectByType<Spawner>();
        if (spawner != null && spawner.gameOverText != null && spawner.gameOverText.gameObject.activeInHierarchy) return false;

        // Only during an active level (timer > 0)
        if (currentTime <= 0f) return false;

        return true;
    }

    private void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenu != null) pauseMenu.SetActive(true);

        if (textCanvasController != null) 
            textCanvasController.TurnOffCanvas();

        // lock inventory buttons while paused
        var inv = FindFirstObjectByType<InventoryUI>();
        if (inv != null) inv.SetMenuLock(true);

        // focus Resume button for controller/keyboard
        var es = EventSystem.current;
        if (es != null && pauseResumeButton != null)
        {
            es.SetSelectedGameObject(null);
            es.SetSelectedGameObject(pauseResumeButton.gameObject);
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenu != null) pauseMenu.SetActive(false);

        if (textCanvasController != null)
            textCanvasController.TurnOnCanvas();

        // unlock inventory again (normal gameplay)
        var inv = FindFirstObjectByType<InventoryUI>();
        if (inv != null) inv.SetMenuLock(false);
    }

    public void ReturnToMainMenu()
    {
        // In the future, call SaveGame function to automatically save the game upon exiting to main menu *********************
        Time.timeScale = 1f; // reset timescale so menu isn’t frozen
        GameSession.Clear();
        SceneManager.LoadScene("MainMenu");
    }

    public void SaveAtLevelComplete()
    {
        var data = new SaveData
        {
            level = GameGrid.level,
            totalScore = GameGrid.totalScore,
            currency = GameGrid.currency,
            savedAtIso = System.DateTime.UtcNow.ToString("o")
        };

        // Player bag
        var spawner = FindFirstObjectByType<Spawner>();
        if (spawner != null && spawner.playerBag != null)
            data.playerBag = new System.Collections.Generic.List<int>(spawner.playerBag.playerBag);

        // Inventory items by name (resolve names from InventoryManager’s current items)
        //var invMgr = FindFirstObjectByType<InventoryManager>();
        //if (invMgr != null && invMgr.items != null)
        //{
        //    foreach (var item in invMgr.items)
        //        if (item != null) data.inventoryItemNames.Add(item.itemName);
        //}
        var invMgr = FindFirstObjectByType<InventoryManager>();
        if (invMgr != null)
        {
            // Active/regular items by name (existing behavior)
            foreach (var item in invMgr.items)
                if (item != null) data.inventoryItemNames.Add(item.itemName);

            // NEW: Passive items by name
            foreach (var p in invMgr.passiveItems)
                if (p != null) data.passiveItemNames.Add(p.itemName);
        }

        SaveSystem.Save(data);
        Debug.Log("Game saved at Level Complete.");
    }

    private void ApplyLoadedDataAndShowLevelComplete(SaveData data)
    {
        // Apply core values
        GameGrid.level = Mathf.Max(1, data.level);
        GameGrid.totalScore = Mathf.Max(0, data.totalScore);
        GameGrid.currency = Mathf.Max(0, data.currency);
        GameGrid.levelUpTriggered = false;

        // Inventory & player bag
        var invMgr = FindFirstObjectByType<InventoryManager>();
        var invUI = FindFirstObjectByType<InventoryUI>();
        
        if (invMgr != null)
        {
            // Clear and rebuild active items existing behavior
            invMgr.items.Clear();
            var shop = FindFirstObjectByType<ItemShopManager>();
            if (shop != null && shop.availableItems != null && data.inventoryItemNames != null)
            {
                foreach (var name in data.inventoryItemNames)
                {
                    var match = System.Array.Find(shop.availableItems, so => so != null && so.itemName == name);
                    if (match != null) invMgr.items.Add(match);
                }
            }

            // Clear and rebuild passive items
            invMgr.passiveItems.Clear();
            if (shop != null && shop.availableItems != null && data.passiveItemNames != null)
            {
                foreach (var name in data.passiveItemNames)
                {
                    var match = System.Array.Find(shop.availableItems, so => so != null && so.itemName == name);
                    if (match != null) invMgr.passiveItems.Add(match);
                }
            }

            if (invUI != null) invUI.RefreshSlots();
        }

        // HUD refresh
        var spawner = FindFirstObjectByType<Spawner>();
        if (spawner != null) spawner.UpdateUI();

        // Clean board in case anything lingered
        GameGrid.ClearGrid();

        // Present the Level Complete UI (paused), like CompleteLevel() does
        Time.timeScale = 0f;
        levelPaused = true;
        levelCompleteMenu.SetActive(true);
        if (levelCompleteText != null)
            levelCompleteText.text = "Level " + GameGrid.level + " Complete!";

        // Focus buttons
        inputArmed = false;
        EventSystem.current.SetSelectedGameObject(null);
        if (continueButton != null)
            EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
        StartCoroutine(ArmMenuSelection());
    }

}