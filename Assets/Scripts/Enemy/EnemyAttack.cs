using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyAttack : MonoBehaviour
{
    //Attack
    public int attackPower = 10;
    public float attackCD = 3;
    private bool playerInRange;
    private PlayerStats player;
    Animator playerAnimator;
    
    Enemy enemy;
    //Stop time feature
    public bool timeStopped;

    void Start() { enemy = GetComponentInParent<Enemy>(); }
    void OnTriggerEnter(Collider col)
    {
        if(col.CompareTag("Player"))
        {
            playerInRange = true;
            enemy._CaughtPlayer  = true;
            player = col.gameObject.GetComponent<PlayerStats>();
            playerAnimator = col.gameObject.GetComponentInChildren<Animator>();

            StartCoroutine(AttackCoolDown());
        }
    }

    IEnumerator AttackCoolDown()
    {
        yield return new WaitForSeconds(attackCD);

        if (playerInRange && 
        !playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack1") && !playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack2") &&
        !enemy.attacked) 
            Attack();
    }

    void Attack()
    {
        if (player.health > 0 && !timeStopped)
        {
            player.health -=attackPower;
            Debug.LogWarning("Your health left:" + player.health);
        }

        StartCoroutine(AttackCoolDown());
    }

    void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Player")) { playerInRange = false; enemy._CaughtPlayer = false; }
    }
}
