using UnityEngine;

public class TileProperties : MonoBehaviour
{
    public bool isGarbage = false; // Determines if this block is a garbage tile
    public bool hasItem = false;

    public void ActivateItem()
    {
        if (hasItem)
        {
            Debug.Log("Item activated from block at " + transform.position);
            // Replace with actual item effects later if we decide to keep items spawning in cleared blocks
            hasItem = false; // Remove item after activation
        }
    }
    // maybe keep to give tiles additional properties?
}
