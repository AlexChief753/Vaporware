using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button newGameButton;
    public Button loadGameButton;
    public Button settingsButton; // placeholder 
    public Button creditsButton; // placeholder 
    public Button exitButton;

    [Header("Scenes")]
    [SerializeField] private string gameplaySceneName = "SampleScene"; // Gameplay scene name

    void Awake()
    {
        // Make sure the game isn't paused if you returned from a prior run
        Time.timeScale = 1f;
    }

    void Start()
    {
        if (newGameButton) newGameButton.onClick.AddListener(StartNewGame);
        if (loadGameButton) loadGameButton.onClick.AddListener(LoadGame);
        if (settingsButton) settingsButton.onClick.AddListener(() => Debug.Log("Settings (placeholder)"));
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

    private void ExitGame()
    {
        Application.Quit();
    }
}

