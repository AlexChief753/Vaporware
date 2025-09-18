using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button newGameButton;
    public Button loadGameButton; // placeholder 
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
        // Wire up buttons
        if (newGameButton) newGameButton.onClick.AddListener(StartNewGame);
        if (loadGameButton) loadGameButton.onClick.AddListener(() => Debug.Log("Load Game (placeholder)"));
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
        SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
    }

    private void ExitGame()
    {
        Application.Quit();
    }
}

