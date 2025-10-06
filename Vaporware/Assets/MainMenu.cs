using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button newGameButton;
    public Button loadGameButton;
<<<<<<< HEAD
    public Button settingsButton; // placeholder 
=======
    public Button settingsButton;
>>>>>>> main
    public Button creditsButton; // placeholder 
    public Button exitButton;

    [Header("Scenes")]
    [SerializeField] private string gameplaySceneName = "SampleScene"; // Gameplay scene name

<<<<<<< HEAD
=======
    [Header("Settings")]
    [SerializeField] private Canvas settingsCanvas;
    [SerializeField] private SettingsController settings;

>>>>>>> main
    void Awake()
    {
        // Make sure the game isn't paused if you returned from a prior run
        Time.timeScale = 1f;
    }

    void Start()
    {
        if (newGameButton) newGameButton.onClick.AddListener(StartNewGame);
        if (loadGameButton) loadGameButton.onClick.AddListener(LoadGame);
<<<<<<< HEAD
        if (settingsButton) settingsButton.onClick.AddListener(() => Debug.Log("Settings (placeholder)"));
=======
        if (settingsButton) settingsButton.onClick.AddListener(OpenSettings);
>>>>>>> main
        if (creditsButton) creditsButton.onClick.AddListener(() => Debug.Log("Credits (placeholder)"));
        if (exitButton) exitButton.onClick.AddListener(ExitGame);

        // Controller focus: land on New Game
        var es = EventSystem.current;
        if (es != null && newGameButton != null)
        {
            es.SetSelectedGameObject(null);
            es.SetSelectedGameObject(newGameButton.gameObject);
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

<<<<<<< HEAD
=======
    private void OpenSettings()
    {
        // ensure parent canvas is active if you keep it disabled in the editor
        if (settingsCanvas && !settingsCanvas.gameObject.activeSelf)
            settingsCanvas.gameObject.SetActive(true);

        // call into the panel's controller (it will set itself active)
        if (settings) settings.Open();
        else Debug.LogWarning("SettingsController not assigned on MainMenu.");
    }

>>>>>>> main
    private void ExitGame()
    {
        Application.Quit();
    }
}

