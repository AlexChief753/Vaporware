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


    void Start()
    {
        AssignRandomItemSlots();
        CreateGhostPiece();
        Invoke("UpdateGhostPiece", 0.05f);
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
        ghostPiece = new GameObject("GhostPiece");
        ghostPiece.tag = "GhostPiece";

        foreach (Transform block in transform)
        {
            GameObject ghostBlock = new GameObject("GhostBlock");
            ghostBlock.transform.SetParent(ghostPiece.transform);
            ghostBlock.transform.localPosition = block.localPosition;

            SpriteRenderer sr = ghostBlock.AddComponent<SpriteRenderer>();
            sr.sprite = ghostSprite;
            sr.color = new Color(1f, 1f, 1f, 0.3f); //1f, 1f, 1f, 0.3f change 0.3f to higher for darker ghost pieces
            sr.sortingOrder = -1;
        }

        ghostPiece.transform.SetParent(null); // Detach ghost piece from Tetromino

        Invoke("UpdateGhostPiece", 0.02f);
    }





    void UpdateGhostPiece()
    {
        if (ghostPiece == null) return; // Safety check

        // Match the rotation of the active Tetromino
        ghostPiece.transform.rotation = transform.rotation;

        // Start from the active Tetromino's position
        Vector3 basePos = transform.position;
        Vector3 finalPos = basePos;

        // Move the ghost down step-by-step until collision
        while (!CheckCollisionAtGhostPosition(finalPos + Vector3.down, ghostPiece.transform.rotation))
        {
            finalPos += Vector3.down;
        }

        // Place the ghost at the final position
        ghostPiece.transform.position = finalPos;
    }




    void Update()
    {
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



    //

    void AdjustFallSpeed()
    {
        float speedIncrease = GameGrid.level * 0.1f; // Increase speed per level
        fallTime = Mathf.Max(0.8f - speedIncrease, minFallTime);
    }


    public static void UpdateGlobalSpeed()
    {
        float speedIncrease = GameGrid.level * 0.1f;
        fallTime = Mathf.Max(baseFallTime - speedIncrease, 0.2f);
    }



    private float moveDelay = 0.2f; // Time between movements when holding key
    private float rotationDelay = 0.25f; // Time between rotations when holding key
    private float lastMoveTime = 0f;
    private float lastRotateTime = 0f;

    void HandleMovement()
    {
        float currentTime = Time.time;

        // Left and Right Movement
        if (Input.GetKey(KeyCode.LeftArrow) && currentTime - lastMoveTime > moveDelay)
        {
            Move(new Vector3(-1, 0, 0));
            lastMoveTime = currentTime;
        }
        else if (Input.GetKey(KeyCode.RightArrow) && currentTime - lastMoveTime > moveDelay)
        {
            Move(new Vector3(1, 0, 0));
            lastMoveTime = currentTime;
        }

        // Rotation (Holding Up Arrow)
        if (Input.GetKey(KeyCode.UpArrow) && currentTime - lastRotateTime > rotationDelay)
        {
            RotateTetromino();
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

        // Soft Drop: Reduce fall time while holding Down Arrow
        if (Input.GetKey(KeyCode.DownArrow))
        {
            currentFallTime = Mathf.Max(fallTime * 0.1f, 0.08f); // Minimum fall time to prevent instant lock
        }

        // Hard Drop: Instantly place the piece
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
            return; // Skip normal falling logic
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
                }

                GameGrid.AddToGrid(this.transform);
                GameGrid.CheckAndClearLines();
                FindFirstObjectByType<Spawner>().SpawnTetromino();
                this.enabled = false; // Disable this piece's movement
            }


        }
    }

    void HardDrop()
    {
        while (!CheckCollision())
        {
            transform.position += new Vector3(0, -1, 0);
        }
        transform.position += new Vector3(0, 1, 0); // Move back up to last valid position
        isLanded = true;

        // Destroy ghost piece if it exists
        if (ghostPiece != null)
        {
            Destroy(ghostPiece);
        }

        GameGrid.AddToGrid(this.transform); // Store piece in grid
        GameGrid.CheckAndClearLines(); // Check for full lines
        FindFirstObjectByType<Spawner>().SpawnTetromino();
        this.enabled = false; // Disable movement after placement
    }

    void OnDestroy()
    {
        if (ghostPiece != null)
        {
            Destroy(ghostPiece);
        }
    }



    void Move(Vector3 direction)
    {
        transform.position += direction;

        if (CheckBoundaryCollision() || CheckCollision())
        {
            transform.position -= direction; // Move back if out of bounds or occupied
        }
    }

    void RotateTetromino()
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
    }

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

    bool CheckCollision()
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

}


