using UnityEngine;

public class ItemSlot : MonoBehaviour
{
    public bool hasItem = false; // Determines if this block has an item

    public void ActivateItem()
    {
        if (hasItem)
        {
            Debug.Log("Item activated from block at " + transform.position);
            // TODO: Replace with actual item effects later
            hasItem = false; // Remove item after activation
        }
    }
}
