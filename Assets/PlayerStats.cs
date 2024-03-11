using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour, IDamageable
{
    public float orig_Health = 100;
    [Space]
    public float orig_Stamina = 30;
    public float staminaChargeSpeed;
    [Space]
    public float orig_Charge = 100;

    [HideInInspector]
    public float health;
    [HideInInspector]
    public float stamina;
    [HideInInspector]
    public float charge;

    public void GiveHealth(float heal)
    {
        health += heal;
    }
    public void TakeDamage(float damage)
    {
        health -= damage;
    }

    public void RecoverStamina(float recovery)
    {
        stamina += recovery;
    }

    public void ReduceStamina(float reduction)
    {
        stamina -= reduction;
        StopAllCoroutines();
        StartCoroutine(RecoveringStamina());
    }


    private IEnumerator RecoveringStamina()
    {
        yield return new WaitForSeconds(3f);
        while (stamina < orig_Stamina)
        {
            stamina += staminaChargeSpeed * Time.deltaTime;
            yield return null;
        }
    }
}
