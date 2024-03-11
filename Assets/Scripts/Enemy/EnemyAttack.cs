using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
[RequireComponent(typeof(EnemyMovement))]
public class EnemyAttack : MonoBehaviour
{
    private float _attackCooldown;
    public bool CanAttack;

    [Space]

    [Header("Debug")]
    [Tooltip("how long attack lasts (more for movement)")]
    [SerializeField]
    private float _attackDuration;
    [SerializeField]
    private Material _attackMaterial;
    private Material _baseMaterial;
    private MeshRenderer _renderer;

    private Enemy _enemy;
    private EnemyMovement _enemyMovement;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _enemyMovement = GetComponent<EnemyMovement>();
        _renderer = GetComponent<MeshRenderer>();
        _attackCooldown = _enemy.EnemyStats.AttackCooldown;
    }

    private void Start()
    {
        _baseMaterial = _renderer.material;

        ResetAttack();
    }

    private void Update()
    {
        if (CanAttack && _enemyMovement.InAttackRange)
            CheckAttack();
            
    }

    //check whether enemy should attack
    private void CheckAttack()
    {
        //debug, attack and wait for attack cooldown before resetting
        CanAttack = false;
        _renderer.material = _attackMaterial;
        _enemyMovement.CanMove = false;
        Debug.Log($"{gameObject.name}: Attacking!");
        Invoke(nameof(FinishAttack), _attackDuration);
        Invoke(nameof(ResetAttack), _attackCooldown);
    }

    //finish attack
    private void FinishAttack()
    {
        _renderer.material = _baseMaterial; //debug color thing
        _enemyMovement.CanMove = true;
    }

    //reset attack cooldown
    private void ResetAttack()
    {
        CanAttack = true;
    }
}
