using UnityEngine;

public class Tetromino : MonoBehaviour
{
    private static float baseFallTime = 0.8f;
    private static float fallTime = baseFallTime;
    private float minFallTime = 0.2f; // Prevents the game from becoming too fast
    private float previousTime;
    private bool isLanded = false;
    private GameObject ghostPiece;
    public Sprite ghostSprite; // Assign the gray square sprite in the Inspector
    //test line
    public int pieceIndex; // set by Spawner when spawning

    // Blocks accidental Space/Submit carry-over for a brief moment after unpausing.
    private static float inputGuardUntilRealtime = 0f;
    private static bool wasPausedLastFrame = false;
<<<<<<< HEAD
=======

    public Sprite ghostSprite1;
    public Sprite ghostSprite2;
    public Sprite ghostSprite3;
    public Sprite ghostSprite4;

    // public int pieceIndex; // set by Spawner when spawning

    private float moveDelay = 0.2f; // Time between movements when holding key
    private float rotationDelay = 0.25f; // Time between rotations when holding key
    private float lastMoveTime = 0f;
    private float lastRotateTime = 0f;

    private static bool spaceKeyReady = true;
>>>>>>> main


    void Start()
    {
        AssignRandomItemSlots();
        AssignGhostSprites();
        CreateGhostPiece();
        Invoke("UpdateGhostPiece", 0.05f);
    }


    void AssignGhostSprites()
    {
        // Try to find sprites by name in Resources folder
        ghostSprite1 = Resources.Load<Sprite>("OneWidthGhostPiece");
        ghostSprite2 = Resources.Load<Sprite>("TwoWidthGhostPiece");
        ghostSprite3 = Resources.Load<Sprite>("ThreeWidthGhostPiece");
        ghostSprite4 = Resources.Load<Sprite>("FourWidthGhostPiece");
        
        Debug.LogWarning($"Programmatic assignment - 1: {ghostSprite1 != null}, 2: {ghostSprite2 != null}, 3: {ghostSprite3 != null}, 4: {ghostSprite4 != null}");
    }


    void AssignRandomItemSlots()
    {
        foreach (Transform block in transform)
        {
            ItemSlot itemSlot = block.gameObject.AddComponent<ItemSlot>();

            // 2% chance to have an item slot
            if (Random.value < 0.2f)
            {
                itemSlot.hasItem = true;
                Debug.Log("Item slot added at " + block.position);
            }
        }
    }


    public void CreateGhostPiece()
    {

        // Clean up any existing ghost piece first
        if (ghostPiece != null)
        {
            Destroy(ghostPiece);
        }

        ghostPiece = new GameObject("GhostPiece");
        // ghostPiece.tag = "GhostPiece";

        // Measure Tetromino Width and select appropriate ghost piece sprite
        float width = GetPieceWidth();
        Sprite selectedGhostSprite = GetGhostSpriteForWidth(width);

        if (selectedGhostSprite == null)
        {
            Debug.LogError("No ghost sprite available! Cannot create ghost piece.");
            Destroy(ghostPiece);
            ghostPiece = null;
            return;
        }

        Debug.Log("Creating ghost piece with width: " + width + ", using sprite: " + selectedGhostSprite.name);

        // Add SpriteRenderer directly to the ghostPiece
        SpriteRenderer sr = ghostPiece.AddComponent<SpriteRenderer>();
        sr.sprite = selectedGhostSprite;
        sr.color = new Color(1f, 1f, 1f, 0.7f); // Add this line for transparency
        sr.sortingOrder = -1;

        float scaleFactor = width / selectedGhostSprite.bounds.size.x;   // Adjusts Width - will need to change if sprite is updated.
        ghostPiece.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
        ghostPiece.transform.SetParent(null);

        // DEBUG: Add a collider to see the bounds in Scene view
        BoxCollider2D collider = ghostPiece.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        Invoke("UpdateGhostPiece", 0.02f);
    }


    private Sprite GetGhostSpriteForWidth(float width)
    {
        int roundedWidth = Mathf.RoundToInt(width);
        
        // Check which sprites are actually assigned and use the first available one
        Sprite[] availableSprites = { ghostSprite1, ghostSprite2, ghostSprite3, ghostSprite4 };
        int index = Mathf.Clamp(roundedWidth - 1, 0, 3);
        
        if (availableSprites[index] != null)
        {
            return availableSprites[index];
        }
        
        // Fallback: use first available sprite
        foreach (Sprite sprite in availableSprites)
        {
            if (sprite != null) 
            {
                Debug.LogWarning($"Ghost sprite for width {roundedWidth} not assigned, using fallback");
                return sprite;
            }
        }
        
        Debug.LogError("No ghost sprites assigned at all!");
        return null;
    }


    // This contains a lot of unecessary code for debugging
    private void UpdateGhostPiece()
    {
        if (ghostPiece == null) return;

        SpriteRenderer sr = ghostPiece.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        float width = GetPieceWidth();
        Sprite newSprite = GetGhostSpriteForWidth(width);

        sr.sprite = newSprite;
        
        // Update scale based on the new width
        float scaleFactor = width / newSprite.bounds.size.x;
        ghostPiece.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);

        // Calculate where the ghost piece should land using the lowest point approach
        Vector3 ghostPosition = CalculateGhostPosition();
        ghostPiece.transform.position = ghostPosition;

        Debug.Log($"Ghost position: {ghostPosition}, Width: {width}, Lowest point: {GetLowestPoint()}");
    }


    void Update()
    {
        // Detect pause/unpause transitions and set a short input guard window
        bool pausedNow = (Time.timeScale == 0f);
        if (pausedNow)
        {
            // Remember we were paused
            wasPausedLastFrame = true;
            return; // you already return while paused - keep that behavior
        }
        else if (wasPausedLastFrame)
        {
            // We just came back from any menu (pause, character select, etc.):
            // disarm Space/Submit for a short moment so we don't hard drop instantly.
            inputGuardUntilRealtime = Time.realtimeSinceStartup + 0.15f; // 150 ms is enough to clear a held key
            wasPausedLastFrame = false;
        }

        // Block all player control while menus/pause are active.
        if (Time.timeScale == 0f) return;

        //If the timer is out, immediately trigger the Game Over flow
        if (LevelManager.instance != null && LevelManager.instance.GetRemainingTime() <= 0f)
        {
            // stop this piece from doing any more logic
            this.enabled = false;

            // clean up ghost so it doesn't linger
            if (ghostPiece != null) Destroy(ghostPiece);

            // call SpawnTetromino() to run the existing IsGameOver() check + Game Over UI
            var spawner = FindFirstObjectByType<Spawner>();
            if (spawner != null) spawner.SpawnTetromino();
            return;
        }

        if (!isLanded) // Only allow movement if the piece hasn't landed yet
        {
            AdjustFallSpeed(); //  Adjust speed based on score
            HandleMovement();
            HandleFalling();
            UpdateGhostPiece(); // Refresh ghost position every frame

        }
    }

    bool CheckCollisionAtGhostPosition(Vector3 ghostPosition, Quaternion ghostRotation)
    {
        // For each block in the *active* Tetromino, figure out where it would be if it had "ghostRotation" at "ghostPosition"
        foreach (Transform block in transform)
        {
            // Rotate the block's local position
            Vector3 rotatedLocalPos = ghostRotation * block.localPosition;

            Vector2 checkPos = new Vector2(
                Mathf.Round(ghostPosition.x + rotatedLocalPos.x),
                Mathf.Round(ghostPosition.y + rotatedLocalPos.y)
            );

            // If any block goes below the grid
            if (checkPos.y < 0)
            {
                return true;
            }

            // If inside the grid and the cell is occupied
            if (checkPos.y < GameGrid.height && GameGrid.IsCellOccupied(checkPos))
            {
                return true;
            }
        }
        return false; // No collision, safe to move down
    }


    void AdjustFallSpeed()
    {
        float speedIncrease = GameGrid.level * InventoryManager.itemSpeedMod * 0.1f; // Increase speed per level
        fallTime = Mathf.Max(0.8f - speedIncrease, minFallTime);
    }


    public static void UpdateGlobalSpeed()
    {
        float speedIncrease = GameGrid.level * InventoryManager.itemSpeedMod * 0.1f;
        fallTime = Mathf.Max(baseFallTime - speedIncrease, 0.2f);
    }


    void HandleMovement()
    {
        float currentTime = Time.time;
        float dpadX = Input.GetAxisRaw("DPadX");
        float dpadY = Input.GetAxisRaw("DPadY");

        // Left and Right Movement
        if ((Input.GetKey(KeyCode.LeftArrow) || dpadX < 0) && currentTime - lastMoveTime > moveDelay)
        {
            Move(new Vector3(-1, 0, 0));
            lastMoveTime = currentTime;
        }
        else if ((Input.GetKey(KeyCode.RightArrow) || dpadX > 0) && currentTime - lastMoveTime > moveDelay)
        {
            Move(new Vector3(1, 0, 0));
            lastMoveTime = currentTime;
        }

        // Rotation Clockwise (Holding Up Arrow)
        if ((Input.GetKey(KeyCode.UpArrow) || dpadY > 0 || Input.GetButton("RotateCCW")) && currentTime - lastRotateTime > rotationDelay)
        {
            RotateTetrominoClockwise();
            lastRotateTime = currentTime;
        }

        if ((Input.GetKey(KeyCode.Return) || Input.GetButton("RotateCW")) && currentTime - lastRotateTime > rotationDelay)
        {
            RotateTetrominoCounterClockwise();
            lastRotateTime = currentTime;
        }
    }

    public void AllowInitialFall()
    {
        // Ensure that the Tetromino can fall immediately, even if it starts above the grid
        previousTime = Time.time - fallTime;
    }


    void HandleFalling()
    {
        float currentFallTime = fallTime;
        float dpadY = Input.GetAxisRaw("DPadY");

        // Soft Drop: Reduce fall time while holding Down Arrow
        if (Input.GetKey(KeyCode.DownArrow) || dpadY < -0.5f)
        {
            currentFallTime = Mathf.Max(fallTime * 0.1f, 0.08f); // Minimum fall time to prevent instant lock
        }

<<<<<<< HEAD
        // Hard Drop: Instantly place the piece
        // Ignore Space/Submit if we're within the post-unpause guard window
        if (Time.realtimeSinceStartup >= inputGuardUntilRealtime)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("HardDrop"))
            {
                HardDrop();
                return;
            }
        }
=======
>>>>>>> main

        // Hard Drop: Instantly place the piece
        // Ignore Space/Submit if we're within the post-unpause guard window
        if (Time.realtimeSinceStartup >= inputGuardUntilRealtime)
        {
            // Prevent accidental super fast drop of next piece by checking for a space key release between pieces
            if (Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp("HardDrop"))
            {
                spaceKeyReady = true;
            }
            
            // Super fast drop!
            else if ((Input.GetKey(KeyCode.Space) || Input.GetButton("HardDrop"))&& spaceKeyReady)
            {
                currentFallTime = Mathf.Max(fallTime * 0.005f, 0.005f);
            }
        }
        // Normal falling logic
        if (Time.time - previousTime > currentFallTime)
        {
            transform.position += new Vector3(0, -1, 0);
            previousTime = Time.time;

            if (CheckCollision())
            {
                transform.position += new Vector3(0, 1, 0); // Move back up to the last valid position
                isLanded = true;

                if (ghostPiece != null)
                {
                    Destroy(ghostPiece); // Destroy ghost of active tetromino
                    ghostPiece = null;
                }

                GameGrid.AddToGrid(this.transform);
                GameGrid.CheckAndClearLines();
                FindFirstObjectByType<Spawner>().SpawnTetromino();
                this.enabled = false; // Disable this piece's movement
                spaceKeyReady = false;      // Disable super fast drops
            }
        }
    }


    void OnDestroy()
    {
        if (ghostPiece != null)
        {
            Destroy(ghostPiece);
            ghostPiece = null;
        }
    }


    void Move(Vector3 direction)
    {
        transform.position += direction;

        if (CheckBoundaryCollision() || CheckCollision())
        {
            transform.position -= direction; // Move back if out of bounds or occupied
        }
        else
        {
            // Successful move - update ghost position
            if (!isLanded && ghostPiece != null)
            {
                UpdateGhostPiece();
            }
        }
    }

    void RotateTetrominoClockwise()
    {
        // Prevent rotation for the O Tetromino
        if (gameObject.name.Contains("O"))
        {
            return; // Do nothing if this is the O piece
        }

        transform.Rotate(0, 0, 90);

        if (CheckBoundaryCollision() || CheckCollision())
        {
            // Try wall kick offsets
            if (!TryWallKick())
            {
                transform.Rotate(0, 0, -90); // Undo rotation if all attempts fail
            }
        }

        // Only update ghost if it exists and we're not landed
        if (!isLanded && ghostPiece != null)
        {
            UpdateGhostPiece();
        }
    }

    void RotateTetrominoCounterClockwise()
    {
        // Prevent rotation for the O Tetromino
        if (gameObject.name.Contains("O"))
        {
            return; // Do nothing if this is the O piece
        }

        transform.Rotate(0, 0, -90);

        if (CheckBoundaryCollision() || CheckCollision())
        {
            // Try wall kick offsets
            if (!TryWallKick())
            {
                transform.Rotate(0, 0, 90); // Undo rotation if all attempts fail
            }
        }
<<<<<<< HEAD
    }

=======

        // Update ghost immediately after rotation
        if (!isLanded && ghostPiece != null)
        {
            UpdateGhostPiece(); // This should recalculate width and position
        }
    }


>>>>>>> main
    bool TryWallKick()
    {
        Vector3[] wallKickOffsets;

        if (gameObject.name.Contains("I")) // Check if this is the I Tetromino
        {
            wallKickOffsets = new Vector3[]
            {
            new Vector3(-2, 0, 0),  // Try moving 2 left
            new Vector3(2, 0, 0),   // Try moving 2 right
            new Vector3(-1, 0, 0),  // Try moving 1 left
            new Vector3(1, 0, 0),   // Try moving 1 right
            new Vector3(0, -1, 0),  // Try moving down slightly
            new Vector3(-1, -1, 0), // Try diagonal left-down
            new Vector3(1, -1, 0)   // Try diagonal right-down
            };
        }
        else
        {
            wallKickOffsets = new Vector3[]
            {
            new Vector3(-1, 0, 0), // Try moving left
            new Vector3(1, 0, 0),  // Try moving right
            new Vector3(0, -1, 0), // Try moving down slightly
            new Vector3(-1, -1, 0), // Try diagonal left-down
            new Vector3(1, -1, 0)  // Try diagonal right-down
            };
        }

        foreach (Vector3 offset in wallKickOffsets)
        {
            transform.position += offset;
            if (!CheckBoundaryCollision() && !CheckCollision())
            {
                return true; // Successful wall kick
            }
            transform.position -= offset; // Undo the move if it still collides
        }

        return false; // No valid wall kick found
    }


    bool CheckBoundaryCollision()
    {
        foreach (Transform child in transform)
        {
            Vector2 pos = new Vector2(Mathf.Round(child.position.x), Mathf.Round(child.position.y));

            if (pos.x < 0 || pos.x >= GameGrid.width) // Check width bounds
            {
                return true;
            }
        }
        return false;
    }


    private bool CheckCollision()
    {
        foreach (Transform child in transform)
        {
            Vector2 pos = new Vector2(Mathf.Round(child.position.x), Mathf.Round(child.position.y));

            // If the piece has reached the bottom of the grid
            if (pos.y < 0)
            {
                return true;
            }

            // If the piece has landed on another block
            if (GameGrid.IsCellOccupied(pos))
            {
                return true;
            }
        }
        return false;
    }


    // There may be a redundant Rounding function call in another function now that this function also calls Round()
    float GetPieceWidth()
    {
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        
        foreach (Transform block in transform)
        {
            Vector3 worldPos = block.position;
            if (worldPos.x < minX) minX = worldPos.x;
            if (worldPos.x > maxX) maxX = worldPos.x;
        }
        
        // Just get the width, no center calculations needed
        float width = Mathf.Round(maxX - minX + 1f);
        return width;
    }


    private bool CheckGhostCollision(Vector3 position)
    {
        // Check the ACTUAL tetromino footprint at this position, not the rectangular bounding box
        foreach (Transform block in transform)
        {
            // Calculate where this block would be if the tetromino was at this position
            Vector3 blockOffset = block.position - transform.position;
            Vector3 blockWorldPos = position + blockOffset;
            
            Vector2 checkPos = new Vector2(
                Mathf.Round(blockWorldPos.x),
                Mathf.Round(blockWorldPos.y)
            );

            // Check bounds
            if (checkPos.x < 0 || checkPos.x >= GameGrid.width || checkPos.y < 0)
            {
                return true;
            }

            // Check if occupied (but only if within grid height)
            if (checkPos.y < GameGrid.height && GameGrid.IsCellOccupied(checkPos))
            {
                return true;
            }
        }
        return false;
    }


    private float GetLowestPoint()
    {
        float lowestY = float.MaxValue;
        
        foreach (Transform block in transform)
        {
            Vector3 worldPos = block.position;
            if (worldPos.y < lowestY) lowestY = worldPos.y;
        }
        
        return Mathf.Round(lowestY);
    }


    private Vector3 CalculateGhostPosition()
    {
        float width = GetPieceWidth();
        float lowestPoint = GetLowestPoint();
        
        // Calculate the tetromino's center for X positioning
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        foreach (Transform block in transform)
        {
            Vector3 worldPos = block.position;
            if (worldPos.x < minX) minX = worldPos.x;
            if (worldPos.x > maxX) maxX = worldPos.x;
        }
        float centerX = (minX + maxX) / 2f;
        
        // Start from the lowest point and find the highest collision in the landing zone
        Vector3 testPosition = new Vector3(centerX, lowestPoint, 0);
        
        // Move down until we find the top of the landing surface
        while (!CheckRectangularCollision(testPosition, width))
        {
            testPosition += Vector3.down;
        }
        
        // Move up to sit on top of the collision surface
        testPosition += Vector3.up;
        
        Debug.Log($"Lowest point: {lowestPoint}, Ghost position: {testPosition}, Width: {width}");
        
        return testPosition;
    }


    private bool CheckRectangularCollision(Vector3 position, float width)
    {
        // Check a rectangular area matching the ghost sprite width
        for (float x = -width/2f + 0.33f; x <= width/2f - 0.33f; x += 1f)
        {
            Vector2 checkPos = new Vector2(
                Mathf.Round(position.x + x),
                Mathf.Round(position.y)
            );

            // Skip if outside horizontal bounds
            if (checkPos.x < 0 || checkPos.x >= GameGrid.width)
                continue;

            // Check if this position would collide
            if (checkPos.y < 0 || (checkPos.y < GameGrid.height && GameGrid.IsCellOccupied(checkPos)))
            {
                return true;
            }
        }
        return false;
    }
    
}

