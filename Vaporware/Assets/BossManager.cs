using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    public Boss[] bosses;
    public Boss currentBoss;
    public static float bossSpeedMod;
    public bool rage;
    public bool BossActive = true;

    public float[] counters; 
    // index 0 is dedicated to a line being cleared

    // preloads any unique boss effects like speed, 
    // and resets conditions
    public void LoadBoss()
    {
        BossActive = true;
        if (currentBoss.bossName == "Jammed Printer")
        {
            bossSpeedMod = 1;
            rage = false;
            counters[0] = 0;
            counters[1] = 0;
        }

        if (currentBoss.bossName == "Annoying Coworker")
        {
            bossSpeedMod = (float) .7;
            rage = false;
            counters[1] = 0;
            counters[2] = 0;
            counters[3] = 0;
        }

        if (currentBoss.bossName == "Tight Deadline")
        {
            rage = false;
            counters[1] = 255; //45 seconds to clear
        }

        if (currentBoss.bossName == "Micromanager")
        {
            rage = false;
            counters[1] = 0;
            counters[2] = 0;
            counters[3] = 0;
        }

        if (currentBoss.bossName == "General Manager")
        {
            rage = false;
            counters[1] = 0;
            counters[2] = 0;
            counters[3] = 0;
        }

        if (currentBoss.bossName == "The CEO")
        {
            rage = false;
            counters[1] = 0;
            counters[2] = 0;
            counters[3] = 0;
        }
    }

    // load rage effects when condition is met
    public void BossRage()
    {
        if (currentBoss.bossName == "Jammed Printer")
        {
            bossSpeedMod = 2;
        }

        if (currentBoss.bossName == "Annoying Coworker")
        {
            bossSpeedMod = (float) .7; // maybe change/remove?
            counters[0] = 0;
        }

        if (currentBoss.bossName == "Tight Deadline")
        {
            var levelMan = FindFirstObjectByType<LevelManager>();
            bossSpeedMod = (float).7;
            if (levelMan.GetRemainingTime() < 240)
                counters[1] = 240;
            else
                counters[1] = levelMan.GetRemainingTime() + 15;
        }

        if (currentBoss.bossName == "Micromanager")
        {
            bossSpeedMod = (float)1.25;
        }

        if (currentBoss.bossName == "General Manager")
        {

        }

        if (currentBoss.bossName == "The CEO")
        {
            var levelMan = FindFirstObjectByType<LevelManager>();
            counters[1] = levelMan.GetRemainingTime() - 30;
            bossSpeedMod = (float)1.2;
            counters[1] = 0;
            counters[2] = 0;
            counters[3] = 0;
        }
    }

    // use to trigger things that should happen
    // on piece drop like garbage spawning
    public void BossPieceDrop()
    {
        if (BossActive == false)
            return;

        var levelMan = FindFirstObjectByType<LevelManager>();
        var spawner = FindFirstObjectByType<Spawner>();
        
        if (currentBoss.bossName == "Jammed Printer")
        {
            if ((GameGrid.requiredScore / 2) < GameGrid.levelScore)
            {
                rage = true;
                BossRage();
            }

            if ((levelMan.GetRemainingTime()) / 10 < counters[1])
            {
                if (counters[0] != 0)
                    counters[0] = 0;

                else
                {

                    if (rage)
                    {
                        if (InventoryManager.GarbageDef < Random.Range(0, 10))
                        {
                            spawner.GarbageLine(Random.Range(0, 10), 0);
                            spawner.GarbageLine(Random.Range(0, 10), 0);
                        }
                    }
                    else if (InventoryManager.GarbageDef < Random.Range(0, 10))
                        spawner.GarbageLine(Random.Range(0, 10), 0);
                }
            }
            counters[1] = Mathf.Floor((levelMan.GetRemainingTime()) / 10);
        }

        if (currentBoss.bossName == "Annoying Coworker")
        {
            if ((GameGrid.requiredScore / 2) < GameGrid.levelScore)
            {
                rage = true;
                BossRage();
            }

            if (rage)
            {
                if (counters[3] > 5)
                {
                    counters[1] = Random.Range(0, 10);
                    counters[2] = GameGrid.GetColumnHeight(Mathf.RoundToInt(counters[1]));
                    if (counters[2] < 20)
                        if (GameGrid.TilesInRow(Mathf.RoundToInt(counters[2])) < 10)
                        {
                            if (InventoryManager.GarbageDef < Random.Range(0, 10))
                                spawner.AddGarbage(Mathf.RoundToInt(counters[2]), Mathf.RoundToInt(counters[1]));
                            counters[3] = 0;
                        }
                }
            }

            else
            {
                if ((levelMan.GetRemainingTime()) / 15 < counters[1])
                {
                    if (InventoryManager.GarbageDef < Random.Range(0, 10))
                        spawner.GarbageLineAnnoying(Random.Range(0,2), 0);
                }
                counters[1] = Mathf.Floor((levelMan.GetRemainingTime()) / 15);
            }
            counters[3]++;
            
        }

        if (currentBoss.bossName == "Tight Deadline")
        {
            if (((3 * GameGrid.requiredScore / 4) < GameGrid.levelScore) && !rage)
            {
                rage = true;
                BossRage();
            }

            if (levelMan.GetRemainingTime() < counters[1])
                if (InventoryManager.GarbageDef < Random.Range(0, 10))
                    spawner.GarbageLine(Random.Range(0, 10), 0);
        }

        if (currentBoss.bossName == "Micromanager")
        {
            if (counters[3] > 6)
            {
                counters[1] = Random.Range(0, 10);
                counters[2] = Random.Range(0, 16);
                if (GameGrid.TilesInRow(Mathf.RoundToInt(counters[2])) < 10)
                {
                    counters[3] = 0;
                    if (InventoryManager.GarbageDef < Random.Range(0, 10))
                    {
                        spawner.AddGarbage(Mathf.RoundToInt(counters[2]), Mathf.RoundToInt(counters[1]));
                    }
                }
            }
            counters[3]++;

            if (((3 * GameGrid.requiredScore / 4) < GameGrid.levelScore) && !rage)
            {
                rage = true;
                BossRage();
            }
        }

        if (currentBoss.bossName == "General Manager")
        {
            if (counters[0] % 2 == 0)
            {

                if (Random.Range(0, 3) < 1 && counters[3] > 10)
                {
                    counters[1] = Random.Range(0, 10);
                    counters[2] = GameGrid.GetColumnHeight(Mathf.RoundToInt(counters[1]));
                    if (GameGrid.TilesInRow(Mathf.RoundToInt(counters[2])) < 10){
                        if (InventoryManager.GarbageDef < Random.Range(0, 10))
                        {
                            if (!rage)
                            {
                                spawner.AddGarbage(Mathf.RoundToInt(counters[2]), Mathf.RoundToInt(counters[1]));
                                spawner.AddGarbage(Mathf.RoundToInt(counters[2]), Mathf.RoundToInt(counters[1] + 1));
                            }
                            else
                            {
                                spawner.AddGarbage(Mathf.RoundToInt(counters[2]), Mathf.RoundToInt(counters[1]));
                                spawner.AddGarbage(Mathf.RoundToInt(counters[2]), Mathf.RoundToInt(counters[1] + 1));
                                spawner.AddGarbage(Mathf.RoundToInt(counters[2]), Mathf.RoundToInt(counters[1] + 2));
                            }
                        }
                        counters[3] -= 5;
                    }
                }
            }
            else if (Random.Range(0, 3) < 1 && counters[3] > 10)
            {
                if (InventoryManager.GarbageDef < Random.Range(0, 10))
                {
                    if (!rage)
                    {
                        spawner.GarbageLine(Random.Range(0, 10), 0);
                    }
                    else
                    {
                        spawner.GarbageLine(Random.Range(0, 10), 0);
                        spawner.GarbageLine(Random.Range(0, 10), 0);
                    }
                }
                counters[3] -= 7;
            }

            counters[3]++;

            if (((3 * GameGrid.requiredScore / 4) < GameGrid.levelScore) && !rage)
            {
                rage = true;
                BossRage();
            }
        }

        if (currentBoss.bossName == "The CEO")
        {
            if ((.33 * GameGrid.requiredScore) < GameGrid.levelScore)
            {
                if (counters[0] % 2 == 0)
                {

                    if (GameGrid.GetHighestOccupiedRow() > 10 && counters[3] > 10)
                    {
                        counters[1] = Random.Range(0, 10);
                        counters[2] = GameGrid.GetColumnHeight(Mathf.RoundToInt(counters[1]));
                        if (GameGrid.TilesInRow(Mathf.RoundToInt(counters[2])) < 10)
                        {
                            if (InventoryManager.GarbageDef < Random.Range(0, 10))
                            {
                                spawner.AddGarbage(Mathf.RoundToInt(counters[2]), Mathf.RoundToInt(counters[1]));
                                spawner.AddGarbage(Mathf.RoundToInt(counters[2]), Mathf.RoundToInt(counters[1] + 1));
                                spawner.AddGarbage(Mathf.RoundToInt(counters[2]), Mathf.RoundToInt(counters[1] + 2));
                            }
                            counters[3] -= 5;
                        }
                    }
                }
                else if (counters[3] > 10)
                {
                    if (InventoryManager.GarbageDef < Random.Range(0, 10))
                    {

                        spawner.GarbageLine(Random.Range(0, 10), 0);

                    }
                    counters[3] -= 7;
                }
            }

            else if (!rage)
            {
                if (counters[3] == 6)
                {
                    counters[1] = Random.Range(0, 10);
                    counters[2] = Random.Range(0, 16);
                    if (GameGrid.TilesInRow(Mathf.RoundToInt(counters[2])) < 10)
                    {
                        if (InventoryManager.GarbageDef < Random.Range(0, 10))
                        {
                            spawner.AddGarbage(Mathf.RoundToInt(counters[2]), Mathf.RoundToInt(counters[1]));
                        }
                    }
                }
                if (counters[3] > 9)
                {
                    if (InventoryManager.GarbageDef < Random.Range(0, 10))
                        spawner.GarbageLine(Random.Range(0, 10), 0);
                    counters[3] = 0;
                }
            }

            if (rage)
            {
                if (levelMan.GetRemainingTime() < counters[1])
                    if (InventoryManager.GarbageDef < Random.Range(0, 10))
                        spawner.GarbageLine(Random.Range(0, 10), 0);

                if ((levelMan.GetRemainingTime()) / 10 < counters[2])
                {
                    if (counters[0] != 0)
                        counters[0] = 0;

                    else
                    {
                        if (InventoryManager.GarbageDef < Random.Range(0, 10))
                        {
                            spawner.GarbageLine(Random.Range(0, 10), 0);
                            spawner.GarbageLine(Random.Range(0, 10), 0);
                        }
                    }
                }
                counters[2] = Mathf.Floor((levelMan.GetRemainingTime()) / 10);
            }

                counters[3]++;

            if (((3 * GameGrid.requiredScore / 4) < GameGrid.levelScore) && !rage)
            {
                rage = true;
                BossRage();
            }
        }
    }

    public void BossDeactivate()
    {
        if (currentBoss.bossName != "The CEO")
        {
            BossActive = false;
            bossSpeedMod = 1;
        }
    }
}
