using UnityEngine;

public class GameGrid : MonoBehaviour
{
    public static int width = 10;
    public static int height = 21; // 10x20 is copyrighted
    public static Transform[,] grid = new Transform[width, height];
    public static int comboCount = 0;
    public static bool comboReset = true;
    public static bool comboProtect = false;
    public static bool comboDropped = false;
    public static double lineClearMult = 1; // Multiplier from line clear amount
    public static double lineClearPoints = 100; // Default line clear value
    public static double fullClearBonus = 1000; // Default full clear bonus
    public static double comboMult = 0.5; // Default combo multiplier
    public static double itemMult = 1; // Base item multiplier
    public static int requiredScore = 0;
    public static float lastLineCleared = 300; // Time last row was cleared
    public static int rowCleared = 0;
    public static float lastPieceLeftness = 1; // For office politics item

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
        lastPieceLeftness = 1;
        foreach (Transform block in tetromino)
        {
            if (block.CompareTag("GhostPiece"))
                continue; // Skip ghost pieces

            Vector2 pos = new Vector2(Mathf.Round(block.position.x), Mathf.Round(block.position.y));
            if (pos.y < height)
            {
                grid[(int)pos.x, (int)pos.y] = block;
            }

            TileProperties tileProperties = block.gameObject.GetComponent<TileProperties>();
            if (tileProperties != null)
                if (tileProperties.isGarbage == false)
                    if (Mathf.Round(block.position.x) <= 5)
                        lastPieceLeftness += (float) 0.25;
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
                TileProperties tileProperties = grid[x, y].GetComponent<TileProperties>();
                if (tileProperties != null)
                {
                        tileProperties.ActivateItem(); // Activate item before removing block
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

    public static void MoveRowsUp(int startY)
    {
        for (int y = height; y > startY; y--)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y - 1] != null)
                {
                    grid[x, y] = grid[x, y - 1]; // Move block up
                    grid[x, y].position += new Vector3(0, 1, 0);
                    grid[x, y - 1] = null;
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
                rowCleared = y;
                y--; // Check the same row again after moving blocks down
                linesCleared++;
                if (level % 4 == 0)
                {
                    var bossMan = FindFirstObjectByType<BossManager>();
                    bossMan.counters[0] = bossMan.counters[0] + 1;
                }
            }
        }

        if (linesCleared > 0) // Handles incrementing and resetting line clear combo
        {
            comboCount++;
            comboProtect = false;
        }
        else if (comboReset && !comboProtect)
        {
            comboCount = 0;
            comboDropped = true;
        }


        //  Award points and check for level-up
        if (linesCleared > 0)
        {
            ScoreAdd(linesCleared);
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
        requiredScore = 500 + (500 * (level - 1)); // Could call GameGrid.level and do some math to increase score required per level ***************************
        if (!levelUpTriggered && levelScore >= requiredScore)
        {
            levelUpTriggered = true; // Prevent multiple triggers
            var levelMan = FindFirstObjectByType<LevelManager>();
            currency += (int)Mathf.Round((float)(levelMan.GetRemainingTime() * level * 1.1) + 100); // Up currency based on remaining time
            var inventoryMan = FindFirstObjectByType<InventoryManager>();
            inventoryMan.PassiveEndRound();
            Time.timeScale = 0; // Pause game immediately
            LevelManager.instance.CompleteLevel();
        }
    }

    public static void UpdateLevelForced() 
    {
        levelUpTriggered = true; // Prevent multiple triggers
        var levelMan = FindFirstObjectByType<LevelManager>();
        var inventoryMan = FindFirstObjectByType<InventoryManager>();
        inventoryMan.PassiveEndRound();
        Time.timeScale = 0; // Pause game immediately
        LevelManager.instance.CompleteLevel();
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

        // Apply per-character score multiplier BEFORE scores are committed or level-up is checked
        points = CharacterEffectsManager.ApplyCharacterScoring(points);

        levelScore += points;
        totalScore += points;

        return points;
    }

    //passive item effects from line clears should be included here
    public static void applyItemEffects(double linesCleared)
    {
        itemMult = 1;
        var inventoryManager = FindFirstObjectByType<InventoryManager>();
        var levelMan = FindFirstObjectByType<LevelManager>();
        for (int i = 0; i < inventoryManager.passiveItems.Count; i++)
        {
            if (inventoryManager.passiveItems[i].itemName == "testItem")
                if (linesCleared == 1)
                    lineClearMult += 0.25;

            if (inventoryManager.passiveItems[i].itemName == "2Lines")
                if (linesCleared == 2)
                    lineClearMult += 0.5;

            if (inventoryManager.passiveItems[i].itemName == "3Lines")
                if (linesCleared == 3)
                    lineClearMult += 1;

            if (inventoryManager.passiveItems[i].itemName == "4Lines")
                if (linesCleared >= 4)
                    lineClearMult += 1.5;

            if (inventoryManager.passiveItems[i].itemName == "Hot Chocolate")
                comboMult = comboMult * 1.25;

            if (inventoryManager.passiveItems[i].itemName == "Coding 4 Clowns")
                lineClearPoints += 100;

            if (inventoryManager.passiveItems[i].itemName == "Expresso")
                if (levelMan.GetRemainingTime() > 270)
                    itemMult = itemMult * ((levelMan.GetRemainingTime() - 270) / 15);

            if (inventoryManager.passiveItems[i].itemName == "Motivational Poster")
            {
                if (GetHighestOccupiedRow() < 10)
                    itemMult = itemMult * ((GetHighestOccupiedRow() / 10) + 1);
                else
                    itemMult = itemMult * 2;
            }

            if (inventoryManager.passiveItems[i].itemName == "CD Player")
                comboMult = comboMult * 0.5;

            if (inventoryManager.passiveItems[i].itemName == "Juggler's Guidebook")
                if (comboDropped == false)
                    comboMult = comboMult * 2;

            if (inventoryManager.passiveItems[i].itemName == "Compressed Air")
                if (IsBoardClear())
                    currency += 100;

            if (inventoryManager.passiveItems[i].itemName == "Play Money")
                if (linesCleared == 3)
                    currency += 20;

            if (inventoryManager.passiveItems[i].itemName == "Schedule Book")
                if (linesCleared == 2)
                    comboCount++;

            if (inventoryManager.passiveItems[i].itemName == "Feather Duster")
                itemMult = itemMult * (2 - (GetHighestOccupiedRow() / 20));

            if (inventoryManager.passiveItems[i].itemName == "Company Card")
                lineClearPoints += currency / 100; // careful tuning with this one, it'll break the game easily if not

            if (inventoryManager.passiveItems[i].itemName == "Pocket Watch")
                itemMult = itemMult * (lastLineCleared - levelMan.GetRemainingTime() / 15);

            if (inventoryManager.passiveItems[i].itemName == "Less is More")
                if (Random.Range(0, 5) < 1)
                {
                    lineClearPoints += 10 * TilesInRow(rowCleared);
                    ClearRow(rowCleared); // rowCleared is now the row above what was cleared
                    MoveRowsDown(rowCleared);
                    rowCleared--;
                    if (rowCleared >= 0)
                    {
                        lineClearPoints += 10 * TilesInRow(rowCleared);
                        ClearRow(rowCleared);
                        MoveRowsDown(rowCleared);
                    }
                }

            if (inventoryManager.passiveItems[i].itemName == "Scrap Paper")
                itemMult = itemMult * (1 + ((float) GarbageCount() / 15));

            if (inventoryManager.passiveItems[i].itemName == "Trash Bag")
            {
                var spawner = FindFirstObjectByType<Spawner>();
                if (levelMan.GetRemainingTime() > lastLineCleared + 10)
                    if (InventoryManager.GarbageDef > Random.Range(0, 10))
                        spawner.GarbageLine(Random.Range(0, 10), 0);
            }

            if (inventoryManager.passiveItems[i].itemName == "Office Politics")
                itemMult = itemMult * lastPieceLeftness;

            if (inventoryManager.passiveItems[i].itemName == "Robot Vaccuum")
                if (Random.Range(0, 10) < 1)
                {
                    lineClearPoints += 10 * TilesInRow(0);
                    ClearRow(0); // rowCleared is now the row above what was cleared
                    MoveRowsDown(0);
                }

            if (InventoryManager.QuadDamActive)
                itemMult = itemMult * 4;
        }

        lastLineCleared = levelMan.GetRemainingTime();
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


    public static int GetColumnHeight(int x)
    {
        
        for (int y = height - 1; y >= 0; y--)
        {
            if (grid[x, y] != null) return y;
        }
        return 0;
    }

    public static int TilesInRow(int y)
    {
        int tiles = 0;
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] != null)
                tiles++;
        }
        return tiles;
    }

    public static int GarbageCount()
    {
        int count = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] != null)
                {
                    TileProperties tileProperties = grid[x, y].gameObject.GetComponent<TileProperties>();
                    if (tileProperties.isGarbage == true)
                        count++;
                }
            }
        }
        return count;
    }

}




