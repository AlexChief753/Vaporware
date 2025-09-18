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

        if (pauseMainMenuButton != null)
            pauseMainMenuButton.onClick.AddListener(() => { Debug.Log("Main Menu (placeholder)"); });

        if (pauseExitButton != null)
            pauseExitButton.onClick.AddListener(QuitGame);
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
        SceneManager.LoadScene("MainMenu");
    }

}

