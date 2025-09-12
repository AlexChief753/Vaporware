using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item", order = 0)]
public class Item : ScriptableObject
{
    [Header("Display")]
    public string itemName;
    public Sprite itemSprite;

    [Header("Shop")]
    public int price = 100;

    [TextArea]
    public string description;

    // public ItemEffectSO effect;
}
