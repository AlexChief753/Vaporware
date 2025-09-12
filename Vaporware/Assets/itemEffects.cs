using UnityEngine;

public class itemEffects : MonoBehaviour
{
    public GameGrid grid;
    public Tetromino tetromino;
    public Item item;
    public void executeEffect(int itemId)
    {
        switch (itemId)
        {
            case 1:
                //effect
                Debug.Log("Item Id 1 Executed");

                //float baseSpeed = Tetromino.fallTime;
                //Debug.Log(baseSpeed);

                //the falltime isn't being updated constantly, fix that
                //Tetromino.fallTime /= 12;
                //Debug.Log(Tetromino.fallTime);
                //after time is up, tetromino.baseFallTime = baseSpeed;
                break;
            case 2:
                //effect
                Debug.Log("Item Id 2 Executed");
                //for (int i = 0; i <2; i++){
                    //grid.ClearRow();
                    //grid.MoveRowsDown();
                //}
                break;
            case 3:
                Debug.Log("Item Id 3 Executed");
                break;
            default:
                break;
        }
    }
}
