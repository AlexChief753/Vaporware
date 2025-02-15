using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
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
    }

    public void ContinueToNextLevel()
    {
        if (levelPaused) // Ensure it only progresses once
        {
            levelPaused = false;
            Time.timeScale = 1; // Resume the game
            GameGrid.level++;
            currentTime = levelTime; // Reset the level timer

            Tetromino.UpdateGlobalSpeed(); // Adjust speed for new level

            // Reset the trigger so the next level-up can happen
            GameGrid.levelUpTriggered = false;

            // Destroy the currently falling Tetromino to prevent merging
            Tetromino activeTetromino = FindFirstObjectByType<Tetromino>();
            if (activeTetromino != null)
            {
                Destroy(activeTetromino.gameObject);
            }

            // Clear the grid before spawning the next Tetromino
            GameGrid.ClearGrid();

            // Find Spawner and update UI
            Spawner spawner = FindFirstObjectByType<Spawner>();
            if (spawner != null)
            {
                spawner.UpdateUI();
                spawner.SpawnTetromino(); // Spawn a new Tetromino for the fresh level
            }
            else
            {
                Debug.LogError("Spawner not found in the scene!");
            }

            levelCompleteMenu.SetActive(false); // Hide the menu
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

