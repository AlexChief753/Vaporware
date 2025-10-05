using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int level; // current level number
    public int totalScore; // total score as of level completion
    public int currency; // currency as of level completion
    public int levelScore;
    public float savedLevelTime;
    public float savedTimeRemaining;
    

    public List<int> playerBag = new List<int>(); // Player.playerBag
    public List<string> inventoryItemNames = new List<string>(); // by name

    public List<string> passiveItemNames = new List<string>();

    public string characterId;

    public List<string> shopItemNames = new List<string>();
    public List<string> shopSoldItemNames = new List<string>();
    public int shopLevelTag;

    public string savedAtIso; // metadata (timestamp)
}
