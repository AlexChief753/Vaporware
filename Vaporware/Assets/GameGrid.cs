using UnityEngine;

public class GameGrid : MonoBehaviour
{
    public static int width = 10;
    public static int height = 21; // 10x20 is copyrighted
    public static Transform[,] grid = new Transform[width, height];
    public static int comboCount = 0;
    public static double lineClearMult = 1; // Multiplier from line clear amount
    public static double lineClearPoints = 100; //Default line clear value
    public static double fullClearBonus = 1000; //Default full clear bonus
    public static double comboMult = 0.5; //Default combo multiplier
    //public static InventoryManager inventoryManager = FindFirstObjectByType<InventoryManager>();

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
    public static int totalScore = 0; // Track total score across the whole game
    public static int levelScore = 0; // Track score within the current level
    public static int level = 1; // Start at level 1
    public static int currency = 0; // Track player currency
    public static void CheckAndClearLines()
    {
        double linesCleared = 0; // Track how many lines were cleared

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

        if (linesCleared > 0) // Handles incrementing and resetting line clear combo
            comboCount++;
        else
            comboCount = 0;


        //  Award points and check for level-up
        if (linesCleared > 0)
        {
            currency += ScoreAdd(linesCleared); // Make currency equal to points
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
        int requiredScore = 1000; // Can call GameGrid.level and do some math to increase score required per level ***************************
        if (!levelUpTriggered && levelScore >= requiredScore)
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
        levelScore = 0;
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

        // Timeout check
        if (LevelManager.instance != null && LevelManager.instance.GetRemainingTime() <= 0f)
        {
            return true; // Game over when the clock runs out
        }

        return false;
    }

    public static int ScoreAdd(double linesCleared)
    {
        lineClearPoints = 100;  // set values to default before modifying with items
        comboMult = 0.5;

        switch (linesCleared) 
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
        else
            fullClearBonus = 0;

        applyItemEffects(linesCleared);

        int points = (int)(((linesCleared * lineClearPoints) + fullClearBonus) *
            lineClearMult * (1 + (comboMult * (comboCount - 1))));

        levelScore += points;
        totalScore += points;

        return points;
    }

    //passive item effects from line clears should be included here
    public static void applyItemEffects(double linesCleared)
    {
        var inventoryManager = FindFirstObjectByType<InventoryManager>();
        for (int i = 0; i < inventoryManager.passiveItems.Count; i++)
        {
            if (inventoryManager.passiveItems[i].itemName == "testItem")
                if (linesCleared == 1)
                    lineClearMult += 0.25;

            if (inventoryManager.passiveItems[i].itemName == "Coding 4 Clowns")
                lineClearPoints += 100;
        }
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

    public static int ClearBottomNRows(int n)
    {
        int cleared = 0;
        for (int i = 0; i < n; i++)
        {
            ClearRow(0); // clear the bottom most row
            MoveRowsDown(0); // shift everything above down by 1
            cleared++;
        }
            return cleared;
    }

    // Returns the highest y that has at least one occupied cell; -1 if board is empty
    public static int GetHighestOccupiedRow()
    {
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] != null) return y;
            }
        }
        return -1;
    }

    // Clears the current top occupied row up to n times (stops early if fewer exist).
    // Returns how many rows were actually cleared.
    public static int ClearTopNOccupiedRows(int n)
    {
        int cleared = 0;
        for (int i = 0; i < n; i++)
        {
            int y = GetHighestOccupiedRow();
            if (y < 0) break; // nothing left
            ClearRow(y); // this also triggers any ItemSlot on those blocks
            MoveRowsDown(y); // pull everything above down by 1
            cleared++;
        }
        return cleared;
    }

}




