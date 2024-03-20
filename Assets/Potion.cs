using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class Potion : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (Vector3.Distance(transform.position, player.transform.position) < 3 && Input.GetKeyDown(KeyCode.E))
        {
            player.GetComponent<IHealable>().Heal(10);
            Destroy(gameObject);
        }
    }
}
