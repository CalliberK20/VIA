using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyStats EnemyStats;

    public float health = 100;
    public Animator animator;

    private bool isDead = false;

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

    //Dead Fuctionn
    private void Dead()
    {
        animator.SetTrigger("Die");
        isDead = true;
    }
}
