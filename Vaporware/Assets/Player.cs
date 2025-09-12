using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public List<int> playerBag = new List<int>();
    
    //Initializes bag.  Can later edit to pass in an index for character starting bags
    public void InitBag()
    {
        // Fill the bag with all possible tetromino indices
        playerBag.Clear();
        for (int i = 0; i < 7; i++) //settinng to 7 initializes it with one of each piece
        {
            playerBag.Add(1);
        }
    }

    public void AddPiece(int index)
    {
        playerBag.Add(index);
    }

}