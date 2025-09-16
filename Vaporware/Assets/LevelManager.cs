using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public TextMeshProUGUI timerText;
    public GameObject levelCompleteMenu;
    public TextMeshProUGUI levelCompleteText;

    private float levelTime = 300f; // 5 minutes per level
    private int scoreRequirement = 1000;
    private float currentTime;
    private bool levelPaused = false;

    public Button continueButton;   // assign in Inspector
    public Button itemShopButton;   // optional
    public Button quitButton;       // optional

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
    }

    void Start()
    {
        currentTime = levelTime;
        UpdateTimerUI();
        levelCompleteMenu.SetActive(false);
    }

    void Update()
    {
        if (!levelPaused)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerUI();

            // Only trigger level completion from the timer
            if (currentTime <= 0)
            {
                CompleteLevel();
            }
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
}

