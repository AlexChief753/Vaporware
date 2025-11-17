using UnityEngine;
using System.Collections.Generic;

public class Tetromino : MonoBehaviour
{
    private static float baseFallTime = 0.8f;
    private static float fallTime = baseFallTime;
    private float minFallTime = 0.2f;
    private float previousTime;
    public bool isLanded = false;
    private GameObject ghostPiece;
    public int pieceIndex; // set by Spawner when spawning

    // Input guard for pause/unpause transitions
    private static float inputGuardUntilRealtime = 0f;
    private static bool wasPausedLastFrame = false;

    // Ghost piece sprites
    public Sprite ghostSprite1;
    public Sprite ghostSprite2;
    public Sprite ghostSprite3;
    public Sprite ghostSprite4;

    // Input timing
    private float moveDelay = 0.2f;
    private float rotationDelay = 0.25f;
    private float lastMoveTime = 0f;
    private float lastRotateTime = 0f;

    private static bool spaceKeyReady = true;

    AudioManager audioManager;

    // Helper component for ghost blocks
    public class GhostBlockData : MonoBehaviour
    {
        public Vector2 localPosition;
    }

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    void Start()
    {
        AssignProperties();
        AssignGhostSprites();
        CreateGhostPiece();
    }

    void AssignGhostSprites()
    {
        ghostSprite1 = Resources.Load<Sprite>("OneWidthGhostPiece");
        ghostSprite2 = Resources.Load<Sprite>("TwoWidthGhostPiece"); 
        ghostSprite3 = Resources.Load<Sprite>("ThreeWidthGhostPiece");
        ghostSprite4 = Resources.Load<Sprite>("FourWidthGhostPiece");
    }


    void AssignProperties()
    {
        foreach (Transform block in transform)
        {
            TileProperties tileProperties = block.gameObject.AddComponent<TileProperties>();

            if (this.pieceIndex == 7)
                tileProperties.isGarbage = true;

            // 2% chance to have an item slot
            //if (Random.value < 0.2f)
            //{
            //    tileProperties.hasItem = true;
            //    Debug.Log("Item slot added at " + block.position);
            //}
        }
    }

    public void CreateGhostPiece()
    {
        if (pieceIndex == 7) return;

        if (ghostPiece != null)
        {
            Destroy(ghostPiece);
            ghostPiece = null;
        }

        ghostPiece = new GameObject("GhostPiece");
        ghostPiece.transform.SetParent(null);

        List<Vector2> bottomBlocks = GetBottomMostBlocks();
        
        foreach (Vector2 bottomPos in bottomBlocks)
        {
            GameObject ghostBlock = new GameObject("GhostBlock");
            ghostBlock.transform.SetParent(ghostPiece.transform);

            SpriteRenderer sr = ghostBlock.AddComponent<SpriteRenderer>();
            sr.sprite = GetSingleBlockGhostSprite();
            sr.color = new Color(1f, 1f, 1f, 0.4f);
            sr.sortingOrder = -1;
            ghostBlock.transform.localScale = Vector3.one;
            
            GhostBlockData data = ghostBlock.AddComponent<GhostBlockData>();
            data.localPosition = bottomPos;
        }

        UpdateGhostPiece();
    }

    private List<Vector2> GetBottomMostBlocks()
    {
        Dictionary<int, float> lowestYByColumn = new Dictionary<int, float>();
        Dictionary<int, List<Vector2>> bottomBlocksByColumn = new Dictionary<int, List<Vector2>>();
        
        foreach (Transform block in transform)
        {
            Vector3 worldPos = block.position;
            int column = Mathf.RoundToInt(worldPos.x);
            float y = worldPos.y;
            
            if (!lowestYByColumn.ContainsKey(column) || y < lowestYByColumn[column])
            {
                lowestYByColumn[column] = y;
                bottomBlocksByColumn[column] = new List<Vector2>();
            }
            
            if (Mathf.Abs(y - lowestYByColumn[column]) < 0.1f)
            {
                Vector3 localPos = block.localPosition;
                bottomBlocksByColumn[column].Add(new Vector2(localPos.x, localPos.y));
            }
        }
        
        List<Vector2> bottomBlocks = new List<Vector2>();
        foreach (var column in bottomBlocksByColumn)
        {
            bottomBlocks.AddRange(column.Value);
        }
        
        // Remove duplicates
        List<Vector2> uniqueBottomBlocks = new List<Vector2>();
        foreach (Vector2 pos in bottomBlocks)
        {
            bool isDuplicate = false;
            foreach (Vector2 existing in uniqueBottomBlocks)
            {
                if (Vector2.Distance(pos, existing) < 0.1f)
                {
                    isDuplicate = true;
                    break;
                }
            }
            if (!isDuplicate)
            {
                uniqueBottomBlocks.Add(pos);
            }
        }
        
        return uniqueBottomBlocks;
    }

    private Sprite GetSingleBlockGhostSprite()
    {
        return ghostSprite1 != null ? ghostSprite1 : CreateSimpleBlockSprite();
    }
    
    private Sprite CreateSimpleBlockSprite()
    {
        int size = 16;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;
        
        for (int i = 0; i < size; i++)
        {
            pixels[i] = Color.white;
            pixels[(size-1)*size + i] = Color.white;
            pixels[i*size] = Color.white;
            pixels[i*size + (size-1)] = Color.white;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
    }

    private void UpdateGhostPiece()
    {
        if (ghostPiece == null || isLanded) return;

        Vector3 ghostPosition = CalculateGhostPosition();
        
        foreach (Transform ghostBlock in ghostPiece.transform)
        {
            GhostBlockData data = ghostBlock.GetComponent<GhostBlockData>();
            if (data != null)
            {
                Vector3 worldOffset = transform.rotation * data.localPosition;
                ghostBlock.position = ghostPosition + worldOffset;
            }
        }
    }

    private Vector3 CalculateGhostPosition()
    {
        if (isLanded) return transform.position;

        Vector3 testPosition = transform.position;
        int safetyCounter = 0;
        const int MAX_ITERATIONS = 30;
        
        while (safetyCounter < MAX_ITERATIONS)
        {
            testPosition.y -= 1f;
            safetyCounter++;
            
            if (CheckCollisionAtPosition(testPosition))
            {
                break;
            }
        }
        
        testPosition.y += 1f;
        return testPosition;
    }

    private bool CheckCollisionAtPosition(Vector3 position)
    {
        foreach (Transform block in transform)
        {
            Vector3 blockOffset = block.position - transform.position;
            Vector3 blockWorldPos = position + blockOffset;
            
            Vector2 checkPos = new Vector2(
                Mathf.Round(blockWorldPos.x),
                Mathf.Round(blockWorldPos.y)
            );

            if (checkPos.x < 0 || checkPos.x >= GameGrid.width || checkPos.y < 0)
                return true;

            if (checkPos.y < GameGrid.height && GameGrid.IsCellOccupied(checkPos))
                return true;
        }
        return false;
    }

    void Update()
    {
        bool pausedNow = (Time.timeScale == 0f);
        if (pausedNow)
        {
            wasPausedLastFrame = true;
            return;
        }
        else if (wasPausedLastFrame)
        {
            inputGuardUntilRealtime = Time.realtimeSinceStartup + 0.15f;
            wasPausedLastFrame = false;
        }

        if (Time.timeScale == 0f) return;

        if (LevelManager.instance != null && LevelManager.instance.GetRemainingTime() <= 0f)
        {
            this.enabled = false;
            if (ghostPiece != null) Destroy(ghostPiece);
            
            var spawner = FindFirstObjectByType<Spawner>();
            if (spawner != null) spawner.SpawnTetromino();
            return;
        }

        if (!isLanded)
        {
            AdjustFallSpeed();
            HandleMovement();
            HandleFalling();
            UpdateGhostPiece();
        }
    }

    void AdjustFallSpeed()
    {
        float speedIncrease = GameGrid.level * InventoryManager.itemSpeedMod * BossManager.bossSpeedMod * 0.1f;
        fallTime = Mathf.Max(0.8f - speedIncrease, minFallTime);
    }

    public static void UpdateGlobalSpeed()
    {
        float speedIncrease = GameGrid.level * InventoryManager.itemSpeedMod * BossManager.bossSpeedMod * 0.1f;
        fallTime = Mathf.Max(baseFallTime - speedIncrease, 0.2f);
    }

    void HandleMovement()
    {
        float currentTime = Time.time;
        float dpadX = Input.GetAxisRaw("DPadX");
        float dpadY = Input.GetAxisRaw("DPadY");

        if ((Input.GetKey(KeyCode.LeftArrow) || dpadX < 0) && currentTime - lastMoveTime > moveDelay)
        {
            audioManager.playSFX(audioManager.pieceMove);
            Move(new Vector3(-1, 0, 0));
            lastMoveTime = currentTime;
        }
        else if ((Input.GetKey(KeyCode.RightArrow) || dpadX > 0) && currentTime - lastMoveTime > moveDelay)
        {
            audioManager.playSFX(audioManager.pieceMove);
            Move(new Vector3(1, 0, 0));
            lastMoveTime = currentTime;
        }

        if ((Input.GetKey(KeyCode.UpArrow) || dpadY > 0 || Input.GetButton("RotateCCW")) && currentTime - lastRotateTime > rotationDelay)
        {
            audioManager.playSFX(audioManager.rotate);
            RotateTetrominoClockwise();
            lastRotateTime = currentTime;
        }

        if ((Input.GetKey(KeyCode.Return) || Input.GetButton("RotateCW")) && currentTime - lastRotateTime > rotationDelay)
        {
            audioManager.playSFX(audioManager.rotate);
            RotateTetrominoCounterClockwise();
            lastRotateTime = currentTime;
        }
    }

    public void AllowInitialFall()
    {
        previousTime = Time.time - fallTime;
    }

    void HandleFalling()
    {
        float currentFallTime = fallTime;
        float dpadY = Input.GetAxisRaw("DPadY");

        if (Input.GetKey(KeyCode.DownArrow) || dpadY < -0.5f)
        {
            currentFallTime = Mathf.Max(fallTime * 0.1f, 0.08f);
        }

        if (Time.realtimeSinceStartup >= inputGuardUntilRealtime)
        {
            if (Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp("HardDrop"))
            {
                spaceKeyReady = true;
            }
            else if ((Input.GetKey(KeyCode.Space) || Input.GetButton("HardDrop")) && spaceKeyReady)
            {
                currentFallTime = Mathf.Max(fallTime * 0.0005f, 0.0005f);
            }
        }
        
        if (Time.time - previousTime > currentFallTime)
        {
            transform.position += new Vector3(0, -1, 0);
            previousTime = Time.time;

            if (CheckCollision())
            {
                transform.position += new Vector3(0, 1, 0);
                isLanded = true;

                if (ghostPiece != null)
                {
                    Destroy(ghostPiece);
                    ghostPiece = null;
                }

                GameGrid.AddToGrid(this.transform);
                GameGrid.CheckAndClearLines();
                
                Spawner spawner = FindFirstObjectByType<Spawner>();
                if (spawner != null) 
                {
                    spawner.SpawnTetromino();
                }
                
                this.enabled = false;
                spaceKeyReady = false;
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
            transform.position -= direction;
        }
        else if (!isLanded && ghostPiece != null)
        {
            UpdateGhostPiece();
        }
    }

    void RotateTetrominoClockwise()
    {
        if (gameObject.name.Contains("O")) return;

        transform.Rotate(0, 0, 90);

        if (CheckBoundaryCollision() || CheckCollision())
        {
            if (!TryWallKick())
            {
                transform.Rotate(0, 0, -90);
            }
        }

        if (!isLanded)
        {
            CreateGhostPiece();
        }
    }

    void RotateTetrominoCounterClockwise()
    {
        if (gameObject.name.Contains("O")) return;

        transform.Rotate(0, 0, -90);

        if (CheckBoundaryCollision() || CheckCollision())
        {
            if (!TryWallKick())
            {
                transform.Rotate(0, 0, 90);
            }
        }

        if (!isLanded)
        {
            CreateGhostPiece();
        }
    }

    bool TryWallKick()
    {
        Vector3[] wallKickOffsets = gameObject.name.Contains("I") ? 
            new Vector3[] { new Vector3(-2, 0, 0), new Vector3(2, 0, 0), new Vector3(-1, 0, 0), new Vector3(1, 0, 0), new Vector3(0, -1, 0), new Vector3(-1, -1, 0), new Vector3(1, -1, 0) } :
            new Vector3[] { new Vector3(-1, 0, 0), new Vector3(1, 0, 0), new Vector3(0, -1, 0), new Vector3(-1, -1, 0), new Vector3(1, -1, 0) };

        foreach (Vector3 offset in wallKickOffsets)
        {
            transform.position += offset;
            if (!CheckBoundaryCollision() && !CheckCollision())
            {
                return true;
            }
            transform.position -= offset;
        }

        return false;
    }

    bool CheckBoundaryCollision()
    {
        foreach (Transform child in transform)
        {
            Vector2 pos = new Vector2(Mathf.Round(child.position.x), Mathf.Round(child.position.y));
            if (pos.x < 0 || pos.x >= GameGrid.width)
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

            if (pos.y < 0)
            {
                return true;
            }

            if (GameGrid.IsCellOccupied(pos))
            {
                return true;
            }
        }
        return false;
    }
}