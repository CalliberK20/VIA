using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    public ItemSlot[] slot;
    [Space]
    public List<Item> inventory = new List<Item>(new Item[4]);

    private void Start()
    {
        Instance = this;
    }

    public void GiveItem(Item newItem)
    {
        if (inventory.Count >= 4)
            return;
        inventory.Add(newItem);
        ItemSlot useSlot = CheckForEmpty();
        useSlot.itemImage.gameObject.SetActive(true);
    }

    public void RemoveItem(Item item)
    {
        inventory.Remove(item);
    }

    private ItemSlot CheckForEmpty()
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (!slot[i].itemImage.gameObject.activeInHierarchy)
                return slot[i];
        }
        return null;
    }
}
