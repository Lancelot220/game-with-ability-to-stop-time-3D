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
    [HideInInspector] public PlayerStats player;
    Sword sword;
    Animator playerAnimator;
    Animator animator;
    
    Enemy enemy;
    //Stop time feature
    public bool timeStopped;

    void Start() 
    {
        enemy = GetComponentInParent<Enemy>();
        sword = transform.parent.gameObject.GetComponentInChildren<Sword>();
        animator = enemy.animator;
    }
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

    public IEnumerator AttackCoolDown()
    {
        yield return new WaitForSeconds(attackCD);

        if (playerInRange)
            sword.attacking = true; animator.SetTrigger("attack");
    }

    /*void Attack()
    {
        if (player.health > 0 && !timeStopped)
        {
            player.health -=attackPower;
            Debug.LogWarning("Your health left:" + player.health);

            StartCoroutine(Rumble.RumblePulse(0.25f, 1f, 0.5f));
        }

        StartCoroutine(AttackCoolDown());
    }*/

    void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Player")) { playerInRange = false; enemy._CaughtPlayer = false; }
    }
}
