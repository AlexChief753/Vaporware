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
            counters[1] = 0;
        }

        if (currentBoss.bossName == "Annoying Coworker")
        {
            bossSpeedMod = (float) 0.8;
            rage = false;
            counters[1] = 0;
            counters[2] = 0;
        }

        if (currentBoss.bossName == "Tight Deadline")
        {
            rage = false;
            counters[1] = 255; //45 seconds to clear
        }

        if (currentBoss.bossName == "Micromanager")
        {

        }

        if (currentBoss.bossName == "General Manager")
        {

        }

        if (currentBoss.bossName == "The CEO")
        {

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

        }

        if (currentBoss.bossName == "General Manager")
        {

        }

        if (currentBoss.bossName == "The CEO")
        {

        }
    }

    // use to trigger things that should happen
    // on piece drop like garbage spawning
    public void BossPieceDrop()
    {
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
                        spawner.GarbageLine(Random.Range(0, 10), 0);
                        spawner.GarbageLine(Random.Range(0, 10), 0);
                    }
                    else
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

            }

            else
            {
                if ((levelMan.GetRemainingTime()) / 15 < counters[1])
                {
                    spawner.GarbageLineAnnoying(Mathf.RoundToInt(counters[2]), 0);
                    counters[2]++;
                }
            }
            counters[1] = Mathf.Floor((levelMan.GetRemainingTime()) / 10);
        }

        if (currentBoss.bossName == "Tight Deadline")
        {
            if ((3 * GameGrid.requiredScore / 4) < GameGrid.levelScore)
            {
                rage = true;
                BossRage();
            }

            if (levelMan.GetRemainingTime() < counters[1])
                spawner.GarbageLine(Random.Range(0, 10), 0);
        }

        if (currentBoss.bossName == "Micromanager")
        {

        }

        if (currentBoss.bossName == "General Manager")
        {

        }

        if (currentBoss.bossName == "The CEO")
        {

        }
    }
}
