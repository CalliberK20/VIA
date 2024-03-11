using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInteractable : MonoBehaviour
{
    public Item item;

    private bool canBePick = false;

    private void Update()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if(Vector3.Distance(transform.position, player.transform.position) < 3 && Input.GetKeyDown(KeyCode.E))
        {
            Inventory.Instance.GiveItem(item);
            Destroy(gameObject);
        }
    }
}
