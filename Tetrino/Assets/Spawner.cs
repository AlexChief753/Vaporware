using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
public class Spawner : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI levelText;
    public GameObject[] tetrominoes;
    public Player playerBag;
    private List<int> bag = new List<int>();

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
            GameGrid.score = 0;
            GameGrid.level = 1;
            Tetromino.UpdateGlobalSpeed(); // This recalculates fallTime based on level 1

            Time.timeScale = 0;
            if (gameOverText != null)
            {
                gameOverText.gameObject.SetActive(true);
            }
            return;
        }


        if (bag.Count == 0)
        {
            FillBag();
        }

        int randomIndex = bag[0];
        bag.RemoveAt(0);
                                                                                    //made the z position not 0 so it spawns above UI -Brandon
        GameObject newTetromino = Instantiate(tetrominoes[randomIndex], new Vector3(5, 22, 0), Quaternion.identity);

        // Ensure the Tetromino can fall even if it starts above the grid
        Tetromino tetrominoScript = newTetromino.GetComponent<Tetromino>();
        if (tetrominoScript != null)
        {
            tetrominoScript.AllowInitialFall();
        }


        //  Update Score UI
        if (scoreText != null)
        {
            scoreText.text = "Score: " + GameGrid.score.ToString();
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
            bag.Add(playerBag.playerBag[i]);
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
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    void RestartGame()
    {
        // Reset game state for a new run
        GameGrid.score = 0;
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


    public void UpdateUI()
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



}
