using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour, IDamagable, IHealable
{
    public float orig_Health = 100;
    public float regen_Speed = 1f;
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

    private Coroutine runningCorountine;

    private void Start()
    {
        UIManager.instance.healthText.text = orig_Health.ToString("00");
        health = orig_Health;
    }

    public void Heal(float heal)
    {
        health += heal;
        UIManager.instance.healthText.text = health.ToString("00");
    }
    public void Damage(float damage)
    {
        health -= damage;
        UIManager.instance.healthText.text = health.ToString("00");
        if(runningCorountine != null)
        StopCoroutine(runningCorountine);
        runningCorountine = StartCoroutine(RecoverHealth());
    }

    public void RecoverStamina(float recovery)
    {
        stamina += recovery;
    }

    public void ReduceStamina(float reduction)
    {
        stamina -= reduction;
        StopCoroutine(RecoveringStamina());
        StartCoroutine(RecoveringStamina());
    }


    private IEnumerator RecoverHealth()
    {
        yield return new WaitForSeconds(3f);
        while(health < orig_Health)
        {
            health += regen_Speed * Time.deltaTime;
            UIManager.instance.healthText.text = health.ToString("00");
            yield return null;
        }
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
