using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    public Boss[] bosses;
    public Boss currentBoss;
    public static float bossSpeedMod;
    public bool rage;

    public float[] counters; 
    // index 0 is dedicated to a line being cleared

    // preloads any unique boss effects like speed, 
    // and resets conditions
    public void LoadBoss()
    {
        if (currentBoss.bossName == "Jammed Printer")
        {
            bossSpeedMod = 1;
            rage = false;
            counters[0] = 0;
            counters[1] = 1;
        }
    }

    // load rage effects when condition is met
    public void BossRage()
    {
        if (currentBoss.bossName == "Jammed Printer")
        {
            bossSpeedMod = 2;
        }
    }

    // use to trigger things that should happen
    // on piece drop like garbage spawning
    public void BossPieceDrop()
    {
        if (currentBoss.bossName == "Jammed Printer")
        {
            if ((GameGrid.requiredScore / 2) < GameGrid.levelScore)
            {
                rage = true;
                BossRage();
            }

            var levelMan = FindFirstObjectByType<LevelManager>();

            if ((levelMan.GetRemainingTime()) / 10 < counters[1])
            {
                if (counters[0] != 0)
                    counters[0] = 0;

                else
                {
                    var spawner = FindFirstObjectByType<Spawner>();

                    if (rage)
                    {
                        spawner.GarbageLine(Random.Range(0, 10), 0);
                        spawner.GarbageLine(Random.Range(0, 10), 0);
                    }
                    else
                        spawner.GarbageLine(Random.Range(0, 10), 0);
                }
            }
            counters[1] = Mathf.Floor((levelMan.GetRemainingTime()) / 10);
        }
    }
}
