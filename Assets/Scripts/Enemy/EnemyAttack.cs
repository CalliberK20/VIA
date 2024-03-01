using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private float _currentAttackCooldown;
    private bool _isAttacking;

    private Enemy _enemy;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
    }

    private void Start()
    {
        FinishAttack();
    }

    private void Update()
    {
        _currentAttackCooldown -= Time.deltaTime;

        if (_currentAttackCooldown < 0 && !_isAttacking)
            CheckAttack();
            
    }

    //check whether enemy should attack
    private void CheckAttack()
    {
         
    }

    //reset cooldown
    private void FinishAttack()
    {
        _isAttacking = false;
        _currentAttackCooldown = _enemy.EnemyStats.AttackCooldown;
    }
}
