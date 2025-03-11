using UnityEngine;

public class itemEffects : MonoBehaviour
{
    public GameGrid grid;
    public Item item;
    public itemTimer timer;
    public ItemDatabase itemDB;
    public Tetromino tetromino;

    public void Start()
    {
        tetromino = FindObjectOfType<Tetromino>();
    }

    public void executeEffect(int itemId)
    {
        switch (itemId)
        {
            case 1:
                Debug.Log("Coffee(Id 1) Executed: Increase fall speed of tetromino");
                //get item duration
                Item item = itemDB.GetItem(1);
                float duration = item.duration;

                //get base speed
                float baseSpeed = Tetromino.fallTime;
                Debug.Log("Base Speed: " + baseSpeed);

                //calculate new speed
                float itemSpeed = Tetromino.fallTime /= 5;
                Debug.Log("Effect Speed: " + itemSpeed);

                //change to new speed
                tetromino.UpdateFallSpeed(itemSpeed);

                //create timer coroutine. When done do function to go back to default speed
                //Without the lambda (()=>), you get a "can't convert a void into System.Action" error
                StartCoroutine(timer.timer(duration, () => tetromino.UpdateFallSpeed(baseSpeed)));
                break;
            case 2:
                Debug.Log("Shredder(Id 2) Executed: Clears 3 lines");
                for (int i = 0; i < 3; i++)
                {
                    GameGrid.ClearRow(0);
                    GameGrid.MoveRowsDown(0);
                }
                break;
            case 3:
                Debug.Log("Item Id 3 Executed");
                break;
            default:
                break;
        }
    }
}

