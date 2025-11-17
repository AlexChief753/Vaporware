using UnityEngine;
// This is for items that spawn in tetromino blocks and are then cleared from the board and activated automatically
// We probably won't keep this
public class ItemSlot : MonoBehaviour
{
    public bool hasItem = false; // Determines if this block has an item

    public void ActivateItem()
    {
        if (hasItem)
        {
            Debug.Log("Item activated from block at " + transform.position);
            // Replace with actual item effects later if we decide to keep items spawning in cleared blocks
            hasItem = false; // Remove item after activation
        }
    }
}
