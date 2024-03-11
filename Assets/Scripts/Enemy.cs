using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float health = 100;
    public Animator animator;
    [Space]
    public Vector3 atkPoint;
    private bool isDead = false;

    private void Start()
    {
        InvokeRepeating("Attack", 1f, 1f);
    }

    //Getting Hit Function
    public void Hit(float damage)
    {
        if(health <= 0)
        {
            if(!isDead)
            {
                Dead();
            }
        }
        else
        {
            health -= damage;
            animator.SetTrigger("Hit");
        }
    }

    void Attack()
    {
        Collider[] collider = Physics.OverlapSphere(transform.position + atkPoint, 1f);
        foreach(Collider collision in collider)
        {
            if (collision.CompareTag("Player"))
            {
                if (collision.TryGetComponent(out IDamageable playerStat))
                    playerStat.TakeDamage(10);
            }
                
        }
    }

    //Dead Fuctionn
    private void Dead()
    {
        animator.SetTrigger("Die");
        isDead = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + atkPoint, 1f);
    }
}
