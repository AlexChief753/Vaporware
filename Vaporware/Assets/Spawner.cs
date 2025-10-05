using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
public class Spawner : MonoBehaviour
{
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI levelScoreText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI currencyText;
    public GameObject[] tetrominoes;
    public Player playerBag;
    private List<int> bag = new List<int>();
    private Queue<int> forcedQueue = new Queue<int>();
    private bool resetBagAfterForced = false;


    public GameObject gameOverPanel; 
    public Button restartButton; 
    public Button mainMenuButton; 
    public Button quitButton; 

    void Awake()
    {
        // Ensure time is running (in case we came from a paused state)
        Time.timeScale = 1f;

        if (GameSession.startMode == StartMode.NewGame || GameSession.pendingSaveData == null)
        {
            // NEW GAME: hard reset all run state
            GameGrid.totalScore = 0;
            GameGrid.levelScore = 0;
            GameGrid.level = 1;
            GameGrid.currency = 0;
            GameGrid.levelUpTriggered = false;
            GameGrid.comboCount = 0;

            GameGrid.ClearGrid();
            Tetromino.UpdateGlobalSpeed();

            if (gameOverText != null)
                gameOverText.gameObject.SetActive(false);
        }
        else
        {
            // else: load path handled by LevelManager.ApplyLoadedDataAndShowLevelComplete()
        }
    }

    void Start()
    {
        playerBag.InitBag();
        SpawnTetromino();

        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(() =>
        {
            // ensure we don't carry paused time or stale session
            GameSession.Clear(); // keep behavior consistent with restart flow
            Time.timeScale = 1f;
            if (LevelManager.instance != null) // prefer existing path out to Main Menu
                LevelManager.instance.ReturnToMainMenu();
        });
        if (quitButton != null) quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }

    public void SpawnTetromino()
    {
        if (GameGrid.IsGameOver())
        {
            Debug.Log("Game Over!");

            // Immediately clear any pending load and delete save so a restart is a true fresh run
            GameSession.Clear();
            SaveSystem.Delete();

            // Reset static game state
            GameGrid.totalScore = 0;
            GameGrid.levelScore = 0;
            GameGrid.level = 1;
            GameGrid.currency = 0;
            Tetromino.UpdateGlobalSpeed(); // This recalculates fallTime based on level 1

            Time.timeScale = 0;
            if (gameOverText != null)
            {
                gameOverText.gameObject.SetActive(true);
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);

                // Give controller/keyboard focus to Restart
                var es = UnityEngine.EventSystems.EventSystem.current;
                if (es != null && restartButton != null)
                {
                    es.SetSelectedGameObject(null);
                    es.SetSelectedGameObject(restartButton.gameObject);
                }

                SetGameOverButtonsInteractable(false);
                StartCoroutine(ArmGameOverButtons());
            }

            // Optional lock the inventory while on Game Over
            var inv = UnityEngine.Object.FindFirstObjectByType<InventoryUI>();
            if (inv != null) inv.SetMenuLock(true);

            return;
        }


        int randomIndex;
        // if the Forced Sequence (Files) item is used
        if (forcedQueue.Count > 0)
        {
            randomIndex = forcedQueue.Dequeue();

            if (forcedQueue.Count == 0 && resetBagAfterForced)
            {
                resetBagAfterForced = false;
                bag.Clear();
                FillBag(); // reset bag after forced sequence
            }
        }
        else // if Forced Sequence item is not used (most of the time it's not used so this activates)
        {
            if (bag.Count == 0)
            {
                FillBag();
            }
            randomIndex = bag[0];
            bag.RemoveAt(0);
        }

        GameObject newTetromino = Instantiate(tetrominoes[randomIndex], new Vector3(5, 22, 0), Quaternion.identity);

        // Ensure the Tetromino can fall even if it starts above the grid
        Tetromino tetrominoScript = newTetromino.GetComponent<Tetromino>();
        if (tetrominoScript != null)
        {
            tetrominoScript.AllowInitialFall();
            tetrominoScript.pieceIndex = randomIndex;
        }


        UpdateScoreUI();

        // Update Currency UI
        if (currencyText != null)
        {
            currencyText.text = "Currency: " + GameGrid.currency.ToString();
        }

        // Update Speed UI (now based on Level, not score)
        if (speedText != null)
        {
            int speedLevel = GameGrid.level; // Speed should match level progression
            speedText.text = "Speed: " + speedLevel.ToString();
        }


        // Update Level UI
        if (levelText != null)
        {
            levelText.text = "Level: " + GameGrid.level.ToString();
        }

        // Update Timer UI
        if (LevelManager.instance != null)
        {
            LevelManager.instance.UpdateTimerUI();
        }

    }



    private void FillBag()
    {
        // Fill the bag with all possible tetromino indices
        bag.Clear();
        for (int i = 0; i < playerBag.playerBag.Count; i++)
        {
<<<<<<< HEAD
            bag.Add(playerBag.playerBag[i]); /////////////////////////////////////////////////////////
=======
            bag.Add(playerBag.playerBag[i]);
>>>>>>> Alex_Branch
        }

        // Shuffle the bag to prevent predictable patterns
        for (int i = 0; i < bag.Count; i++)
        {
            int temp = bag[i];
            int randomIndex = Random.Range(i, bag.Count);
            bag[i] = bag[randomIndex];
            bag[randomIndex] = temp;
        }
    }


        void Update()
        {
            // Only allow R to restart when the Game Over text is actually shown
            if (gameOverText != null &&
                gameOverText.gameObject.activeInHierarchy &&
                (Input.GetKeyDown(KeyCode.R) || Input.GetButtonDown("Restart")))
            {
                RestartGame();
            }
    }

        void RestartGame()
        {

            // Ensure we do NOT reload into a saved state
            GameSession.Clear();
            SaveSystem.Delete();

            // Reset game state for a new run
            GameGrid.totalScore = 0;
            GameGrid.levelScore = 0;
            GameGrid.level = 1;
            GameGrid.currency = 0;
            GameGrid.comboCount = 0;
            var inventoryManager = FindFirstObjectByType<InventoryManager>();
            inventoryManager.passiveItems.Clear();
            Tetromino.UpdateGlobalSpeed();

            GameGrid.ClearGrid(); // Clear the grid
            Time.timeScale = 1; // Resume the game

            // Hide the Game Over UI
            if (gameOverText != null)
            {
                gameOverText.gameObject.SetActive(false);
            }

            if (gameOverPanel != null) gameOverPanel.SetActive(false);

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    


    public void UpdateUI() // Needed for new level to be updated
    {
        if (levelText != null)
        {
            levelText.text = "Level: " + GameGrid.level.ToString();
        }

        if (speedText != null)
        {
            speedText.text = "Speed: " + GameGrid.level.ToString();
        }
    }

    public void UpdateScoreUI()
    {
        if (totalScoreText != null)
            totalScoreText.text = "Total Score: " + GameGrid.totalScore.ToString();

        if (levelScoreText != null)
            levelScoreText.text = "Level Score: " + GameGrid.levelScore.ToString();
    }

    public void ForceSequence(int pieceIndex, int times)
    {
        forcedQueue.Clear();
        for (int i = 0; i < times; i++)
        {
            forcedQueue.Enqueue(pieceIndex);
        }
        resetBagAfterForced = true;
    }

    private void SetGameOverButtonsInteractable(bool on)
    {
        if (restartButton) restartButton.interactable = on;
        if (mainMenuButton) mainMenuButton.interactable = on;
        if (quitButton) quitButton.interactable = on;
    }

    private System.Collections.IEnumerator ArmGameOverButtons()
    {
        // Wait at least one rendered frame
        yield return new WaitForEndOfFrame();

        // Wait until all "submit" variants are fully released
        // old Input Manager defaults: Submit maps to Return/Enter + joystick button 0 (A)
        while (Input.GetButton("Submit") ||
               Input.GetKey(KeyCode.Return) ||
               Input.GetKey(KeyCode.KeypadEnter) ||
               Input.GetKey(KeyCode.Space) ||
               Input.GetKey(KeyCode.JoystickButton0))
        {
            yield return null;
        }

        // enable and focus the Restart button
        SetGameOverButtonsInteractable(true);

        var es = EventSystem.current;
        if (es && restartButton)
        {
            es.SetSelectedGameObject(null);
            es.SetSelectedGameObject(restartButton.gameObject);
        }
    }

}
