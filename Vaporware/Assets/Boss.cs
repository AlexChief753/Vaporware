using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

[CreateAssetMenu(fileName = "NewBoss", menuName = "Game/Boss", order = 0)]
public class Boss : ScriptableObject
{
    [Header("Attributes")]
    public string bossName;
    public int scoreMultiplier;

    [TextArea]
    public string description;
}
