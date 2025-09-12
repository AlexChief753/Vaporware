using UnityEngine;

public class GameGrid : MonoBehaviour
{
    public static int width = 10;
    public static int height = 21; // 10x20 is copyrighted
    public static Transform[,] grid = new Transform[width, height];
    public static int comboCount = 0;

    public static bool IsInsideGrid(Vector2 pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0;
    }

    public static bool IsCellOccupied(Vector2 pos)
    {
        if (pos.y >= height) return false; // Prevents checking out of bounds
        return grid[(int)pos.x, (int)pos.y] != null;
    }

    public static void AddToGrid(Transform tetromino)
    {
        foreach (Transform block in tetromino)
        {
            if (block.CompareTag("GhostPiece"))
                continue; // Skip ghost pieces

            Vector2 pos = new Vector2(Mathf.Round(block.position.x), Mathf.Round(block.position.y));
            if (pos.y < height)
            {
                grid[(int)pos.x, (int)pos.y] = block;
            }
        }
    }


    public static bool IsRowFull(int y)
    {
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] == null)
            {
                return false; // Row is not full
            }
        }
        return true; // Row is full
    }

    public static bool IsBoardClear()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] != null)
                {
                    return false; // Board is not clear
                }
            }
        }
        return true;
    }
    public static void ClearRow(int y)
    {
    for (int x = 0; x < width; x++)
    {
        if (grid[x, y] != null)
        {
            // Check if the block has an item
            ItemSlot itemSlot = grid[x, y].GetComponent<ItemSlot>();
            if (itemSlot != null)
            {
                itemSlot.ActivateItem(); // Activate item before removing block
            }

            Destroy(grid[x, y].gameObject); // Remove the block
            grid[x, y] = null;
        }
    }
    }


    public static void MoveRowsDown(int startY)
    {
        for (int y = startY; y < height - 1; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y + 1] != null)
                {
                    grid[x, y] = grid[x, y + 1]; // Move block down
                    grid[x, y].position += new Vector3(0, -1, 0);
                    grid[x, y + 1] = null;
                }
            }
        }
    }
    public static int score = 0; // Track player score
    public static int level = 1; // Start at level 1
    public static int currency = 0; // Track player currency
    public static void CheckAndClearLines()
    {
        double linesCleared = 0; // Track how many lines were cleared
        double lineClearMult = 1; // Multiplier from line clear amount
        double fullClearBonus = 0;

        for (int y = 0; y < height; y++)
        {
            if (IsRowFull(y))
            {
                ClearRow(y);
                MoveRowsDown(y);
                y--; // Check the same row again after moving blocks down
                linesCleared++;
            }
        }

        if (linesCleared > 0) // Handles incrementing and resetting line combo
            comboCount++;
        else
            comboCount = 0;


        //  Award points and check for level-up
        if (linesCleared > 0)
        {
            switch (linesCleared) //  Check for point multiplier to apply
            {
                case 1:
                    lineClearMult = 1;
                    break;
                case 2:
                    lineClearMult = 1.25;
                    break;
                case 3:
                    lineClearMult = 1.5;
                    break;
                default:
                    lineClearMult = 2.5;
                    break;
            }
            if (IsBoardClear())
                fullClearBonus = 1000; // Additional 1000 base points for a full clear

            int points = (int)(((linesCleared * 100) + fullClearBonus) *
                lineClearMult * (0.5 + (0.5 * comboCount))); // 100 points per line
            score += points;
            currency += points; // Make currency equal to points
            UpdateLevel(); //  Check if level should increase
        }
    }

    private static void CleanupGhostPieces()
    {
        // Find all objects tagged "GhostPiece"
        var allGhosts = GameObject.FindGameObjectsWithTag("GhostPiece");
        foreach (var ghost in allGhosts)
        {
            // Get the parent tetromino (if any)
            var parentTetromino = ghost.transform.parent != null ? ghost.transform.parent.GetComponent<Tetromino>() : null;
            // If there's no parent tetromino, or its script is disabled, destroy the ghost
            if (parentTetromino == null || !parentTetromino.enabled)
            {
                Object.Destroy(ghost);
            }
        }
    }



    public static bool levelUpTriggered = false; // Prevents multiple level-ups

    public static void UpdateLevel()
    {
        int requiredScore = GameGrid.level * 100; // 1000 points per level ***************************
        if (!levelUpTriggered && score >= requiredScore)
        {
            levelUpTriggered = true; // Prevent multiple triggers
            Time.timeScale = 0; // Pause game immediately
            LevelManager.instance.CompleteLevel();
        }
    }


    // Reset levelUpTriggered when starting a new level
    public static void ResetLevelTrigger()
    {
        levelUpTriggered = false;
    }




    public static bool IsGameOver()
    {
        for (int x = 0; x < width; x++)
        {
            if (grid[x, height - 1] != null) // Check top row for a block
            {
                return true;
            }
        }
        return false;
    }

    public static void ClearGrid()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] != null)
                {
                    Destroy(grid[x, y].gameObject); // Remove block from scene
                    grid[x, y] = null; // Clear reference in the grid
                }
            }
        }
    }


}




