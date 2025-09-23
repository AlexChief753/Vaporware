using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int level; // current level number
    public int totalScore; // total score as of level completion
    public int currency; // currency as of level completion
    

    public List<int> playerBag = new List<int>(); // Player.playerBag
    public List<string> inventoryItemNames = new List<string>(); // by name

    public string savedAtIso; // metadata (timestamp)
}
