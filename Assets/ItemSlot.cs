using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public Image itemImage;

    private void Start()
    {
        itemImage = transform.GetChild(0).GetComponent<Image>();
        itemImage.gameObject.SetActive(false);
    }

    public void DisplayItem()
    {
        itemImage.gameObject.SetActive(true);
    }
}
