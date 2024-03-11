using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    [SerializeField]
    private float _health;

    public void TakeDamage(float damage)
    {
        _health -= damage;

        _health = Mathf.Clamp(_health - damage, 0, float.MaxValue);
    }

}
