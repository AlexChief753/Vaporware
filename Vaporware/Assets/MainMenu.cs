using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button newGameButton;
    public Button loadGameButton;
    public Button settingsButton;
    public Button creditsButton; // placeholder 
    public Button exitButton;

    [Header("Scenes")]
    [SerializeField] private string gameplaySceneName = "SampleScene"; // Gameplay scene name

    [Header("Settings")]
    [SerializeField] private Canvas settingsCanvas;
    [SerializeField] private SettingsController settings;

    [Header("Credits")]
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private Button backButton;
    public GameObject mainMenuPanel;

    void Awake()
    {
        // Make sure the game isn't paused if you returned from a prior run
        Time.timeScale = 1f;
    }

    void Start()
    {
        if (newGameButton) newGameButton.onClick.AddListener(StartNewGame);
        if (loadGameButton) loadGameButton.onClick.AddListener(LoadGame);
        if (settingsButton) settingsButton.onClick.AddListener(OpenSettings);
        if (creditsButton) creditsButton.onClick.AddListener(OpenCredits);
        if (backButton) backButton.onClick.AddListener(CloseCredits);
        if (exitButton) exitButton.onClick.AddListener(ExitGame);

        // Controller focus: land on New Game
        var es = EventSystem.current;
        if (es != null)
        {
            es.SetSelectedGameObject(null);  // start with nothing selected
        }
        //var es = EventSystem.current;
        //if (es != null && newGameButton != null)
        //{
        //    es.SetSelectedGameObject(null);
        //    es.SetSelectedGameObject(newGameButton.gameObject);
        //}
    }

    void Update()
    {
        // If Credits panel is open
        if (creditsPanel != null && creditsPanel.activeInHierarchy)
        {
            // ESC closes credits
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseCredits();
                return;
            }

            // Controller B button (Joypad East)
            if (Input.GetButtonDown("Cancel"))
            {
                CloseCredits();
                return;
            }
        }
    }

    private void StartNewGame()
    {
        GameSession.startMode = StartMode.NewGame;
        GameSession.pendingSaveData = null;
        SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
    }

    private void LoadGame()
    {
        var data = SaveSystem.Load();
        if (data == null)
        {
            Debug.Log("No save found.");
            return;
        }
        GameSession.startMode = StartMode.LoadGame;
        GameSession.pendingSaveData = data;
        SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
    }

    private void OpenSettings()
    {
        // ensure parent canvas is active if you keep it disabled in the editor
        if (settingsCanvas && !settingsCanvas.gameObject.activeSelf)
            settingsCanvas.gameObject.SetActive(true);

        // call into the panel's controller (it will set itself active)
        if (settings) settings.Open();
        else Debug.LogWarning("SettingsController not assigned on MainMenu.");
    }

    private void ExitGame()
    {
        Application.Quit();
    }

    private void OpenCredits()
    {
        // Hide main menu
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        // Show credits panel
        if (creditsPanel != null)
            creditsPanel.SetActive(true);

        // Controller focus moves to Back button
        var es = EventSystem.current;
        if (es != null)
        {
            es.SetSelectedGameObject(null);  // start with nothing selected
        }
    }

    private void CloseCredits()
    {
        // Hide credits
        if (creditsPanel != null)
            creditsPanel.SetActive(false);

        // Show main menu again
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        // Controller/keyboard focus on New Game button
        var es = EventSystem.current;
        if (es != null)
        {
            es.SetSelectedGameObject(null);  // start with nothing selected
        }
    }
}

