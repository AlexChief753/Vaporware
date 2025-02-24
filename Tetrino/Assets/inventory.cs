using UnityEngine;

public class inventory : MonoBehaviour
{
    [SerializeField] private ItemDatabase _itemDatebase;
    //Change number to change how many inventory slots does the character have
    public Item[] inv = new Item[3]; 

    public void useItem()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            //Look into https://docs.unity.com/ugs/manual/economy/manual/SDK-player-inventory
            //function to get itemId of this inventory slot
            //if item exists
                //execute function of item id
            //if item don't exist
                //nothing
        }
        if (Input.GetKeyDown(KeyCode.X))
        {

        }
        if (Input.GetKeyDown(KeyCode.C))
        {

        }
    }

    public void getItemId()
    {

    }

    public void addItem(int itemId, inventory inventory)
    {
        //In the future when we implement the store, make it so this function activates from pressing a button rather than a key

    }

    public void removeItem(int itemId, inventory inventory)
    {

    }
}
