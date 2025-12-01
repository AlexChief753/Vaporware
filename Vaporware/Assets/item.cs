using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item", order = 0)]
public class Item : ScriptableObject
{
    [Header("Display")]
    public string itemName;
    public Sprite itemSprite;
    public bool passive;  // tag this if the item is a passive instead of consumable

    [Header("Shop")]
    public int price = 100;

    [TextArea]
    public string description;

}
