using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static CharacterID;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public TextMeshProUGUI timerText;
    public GameObject levelCompleteMenu;
    public TextMeshProUGUI levelCompleteText;

    [Header("Level Complete Stats")]
    [SerializeField] private TMPro.TextMeshProUGUI lcTotalScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI lcLevelScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI lcTimeText;
    [SerializeField] private TMPro.TextMeshProUGUI lcCurrencyText;

    public float GetLevelTimeConfigured() => levelTime;

    private float levelTime = 300f; // 5 minutes per level is 300f
    private int scoreRequirement = 500;
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
    public Button pauseMainMenuButton;
    public Button pauseExitButton;

    public CanvasController textCanvasController;

    private bool isPaused = false;
    bool inputArmed = false;// gate to ignore first Submit press

    [SerializeField] private Button firstMenuButton;

    [Header("Settings")]
    [SerializeField] private Canvas settingsCanvas;          // parent canvas
    [SerializeField] private SettingsController settings;    // SettingsPanel's controller

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
        UpdateTimerUI();
        levelCompleteMenu.SetActive(false);

        // Hide pause menu and wire buttons
        if (pauseMenu != null) pauseMenu.SetActive(false);

        if (pauseResumeButton != null)
            pauseResumeButton.onClick.AddListener(ResumeGame);

        if (pauseSettingsButton != null)
        {
            pauseSettingsButton.onClick.RemoveAllListeners();
            pauseSettingsButton.onClick.AddListener(OpenSettingsFromPause);
        }

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
        // If settings is open, swallow pause/unpause inputs
        if (settings != null && settings.IsOpen)
        {

        }
        else
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
        }

        if (!levelPaused)
        {
            if (currentTime > 0f)
            {
                currentTime -= Time.deltaTime;
                if (currentTime < 0f) currentTime = 0f; // clamp so the clock stops at 00:00
            }

            UpdateTimerUI();
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
        levelCompleteText.text = GameGrid.level + " Complete!";

        SetLevelCompleteStats();

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
        if (!levelCompleteMenu || !levelCompleteMenu.activeInHierarchy) return;

        // Unlock inventory befor hiding the menu
        var inv = FindFirstObjectByType<InventoryUI>();
        if (inv != null) inv.SetMenuLock(false);

        // Proceed
        levelPaused = false;
        Time.timeScale = 1;
        GameGrid.level++; // Increment level 
        currentTime = levelTime;
        GameGrid.levelScore = 0;

        var inventoryManager = FindFirstObjectByType<InventoryManager>();
        inventoryManager.PassiveInit();
        GameGrid.lastLineCleared = 300;
        BossManager.bossSpeedMod = 1;
        GameGrid.comboCount = 0;

        if (GameGrid.level % 4 == 0)
        { 
            var bossMan = FindFirstObjectByType<BossManager>();
            bossMan.currentBoss = bossMan.bosses[((GameGrid.level - 4) / 4) % 6];
            bossMan.LoadBoss();
        }

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
        // In the future, we could call SaveGame function to automatically save the game upon exiting to main menu *********************
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
            savedAtIso = System.DateTime.UtcNow.ToString("o"),
            characterId = CharacterRuntime.Selected.ToString(),
            levelScore = GameGrid.levelScore,
            savedLevelTime = levelTime,
            savedTimeRemaining = currentTime
        };

        // Player bag
        var spawner = FindFirstObjectByType<Spawner>();
        if (spawner != null && spawner.playerBag != null)
            data.playerBag = new System.Collections.Generic.List<int>(spawner.playerBag.playerBag);

        var invMgr = FindFirstObjectByType<InventoryManager>();
        if (invMgr != null)
        {
            // Active/regular items by name
            foreach (var item in invMgr.items)
                if (item != null) data.inventoryItemNames.Add(item.itemName);

            //Passive items by name
            foreach (var p in invMgr.passiveItems)
                if (p != null) data.passiveItemNames.Add(p.itemName);
        }

        var shopSvc = ShopService.FindOrCreate();
        shopSvc.SaveTo(data);   // saves 4 items, what was sold, level tag

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
        GameGrid.levelScore = Mathf.Max(0, data.levelScore);

        levelTime = data.savedLevelTime;
        currentTime = data.savedTimeRemaining;

        // Inventory & player bag
        var invMgr = FindFirstObjectByType<InventoryManager>();

        var shopSvc = ShopService.FindOrCreate();
        shopSvc.LoadFrom(data); // restores the same 4 items and the ones already sold

        // Restore selected character into runtime
        if (!string.IsNullOrEmpty(data.characterId) &&
            System.Enum.TryParse(data.characterId, out CharacterId loadedId))
        {
            CharacterRuntime.Set(loadedId);
        }
        else
        {
            CharacterRuntime.Set(CharacterId.Trinity);
        }

        // Build a global name, Item lookup
        var byName = new Dictionary<string, Item>();

        // From the shop pool
        var svc = ShopService.FindOrCreate();
        if (svc != null && svc.pool != null && svc.pool.entries != null)
        {
            foreach (var e in svc.pool.entries)
            {
                if (e != null && e.item != null && !string.IsNullOrEmpty(e.item.itemName))
                    if (!byName.ContainsKey(e.item.itemName))
                        byName.Add(e.item.itemName, e.item);
            }
        }

        // Also include anything under Resources
        var extras = Resources.LoadAll<Item>(""); // no path = search all under Resources
        foreach (var it in extras)
        {
            if (it != null && !string.IsNullOrEmpty(it.itemName))
                if (!byName.ContainsKey(it.itemName))
                    byName.Add(it.itemName, it);
        }

        // Rebuild the inventory from saved names
        invMgr.items.Clear();
        if (data.inventoryItemNames != null)
        {
            foreach (var name in data.inventoryItemNames)
                if (!string.IsNullOrEmpty(name) && byName.TryGetValue(name, out var item))
                    invMgr.items.Add(item);
        }

        invMgr.passiveItems.Clear();
        if (data.passiveItemNames != null)
        {
            foreach (var name in data.passiveItemNames)
                if (!string.IsNullOrEmpty(name) && byName.TryGetValue(name, out var item))
                    invMgr.passiveItems.Add(item);
        }

        // Make the UI reflect the lists
        var invUI = FindFirstObjectByType<InventoryUI>();
        if (invUI != null) invUI.RefreshSlots();

        // HUD refresh
        var spawner = FindFirstObjectByType<Spawner>();
        if (spawner != null) spawner.UpdateUI();

        // Clean board in case anything lingered
        GameGrid.ClearGrid();

        // Present the Level Complete UI (paused)
        Time.timeScale = 0f;
        levelPaused = true;
        levelCompleteMenu.SetActive(true);
        if (levelCompleteText != null)
            levelCompleteText.text = + GameGrid.level + " Complete!";
        SetLevelCompleteStats();

        // Focus buttons
        inputArmed = false;
        EventSystem.current.SetSelectedGameObject(null);
        if (continueButton != null)
            EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
        StartCoroutine(ArmMenuSelection());
    }

    private void OpenSettingsFromPause()
    {
        if (settingsCanvas && !settingsCanvas.gameObject.activeSelf)
            settingsCanvas.gameObject.SetActive(true);

        if (settings)
        {
            // Hide Pause while settings is open
            if (pauseMenu) pauseMenu.SetActive(false);

            settings.Open(onClosed: () =>
            {
                // Restore Pause when closing Settings
                if (pauseMenu) pauseMenu.SetActive(true);

                // Restore controller focus to Resume button
                var es = EventSystem.current;
                if (es && pauseResumeButton)
                {
                    es.SetSelectedGameObject(null);
                    es.SetSelectedGameObject(pauseResumeButton.gameObject);
                }
            });
        }
        else
        {
            Debug.LogWarning("SettingsController not assigned in LevelManager.");
        }
    }

    //Format mm:ss
    private static string FormatTime(float seconds)
    {
        if (seconds < 0) seconds = 0;
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return $"{m:00}:{s:00}";
    }

    // Push stats into the Level Complete UI texts
    private void SetLevelCompleteStats()
    {
        if (lcTotalScoreText) lcTotalScoreText.text = ": " + GameGrid.totalScore.ToString();
        if (lcLevelScoreText) lcLevelScoreText.text = ": " + GameGrid.levelScore.ToString();

        // elapsed this level = levelTime - currentTime
        float elapsed = Mathf.Clamp(levelTime - currentTime, 0f, levelTime);
        if (lcTimeText) lcTimeText.text = ": " + FormatTime(elapsed);

        if (lcCurrencyText) lcCurrencyText.text = ": " + GameGrid.currency.ToString();
    }

    public void RefreshLevelCompleteUI()
    {
        SetLevelCompleteStats();
    }

}