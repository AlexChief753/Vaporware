using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
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



    void Start()
    {
        playerBag.InitBag();
        SpawnTetromino();
    }

    public void SpawnTetromino()
    {
        if (GameGrid.IsGameOver())
        {
            Debug.Log("Game Over!");

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
            bag.Add(playerBag.playerBag[i]); /////////////////////////////////////////////////////////
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
            // Reset game state for a new run
            GameGrid.totalScore = 0;
            GameGrid.levelScore = 0;
            GameGrid.level = 1;
            Tetromino.UpdateGlobalSpeed();

            Time.timeScale = 1; // Resume the game
            GameGrid.ClearGrid(); // Clear the grid

            // Hide the Game Over UI
            if (gameOverText != null)
            {
                gameOverText.gameObject.SetActive(false);
            }

            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
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

}
