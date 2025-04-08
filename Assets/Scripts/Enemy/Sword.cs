using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    [HideInInspector] public bool attacking;
    public EnemyAttack enemyAttack;

    //void Start() { enemyAttack = transform.parent.gameObject.GetComponentInChildren<EnemyAttack>(); } 

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player") && attacking && enemyAttack.player.health > 0 && !enemyAttack.timeStopped)
        {
            enemyAttack.player.health -=enemyAttack.attackPower;
            enemyAttack.player.StartFallingWhenHit();
            Debug.LogWarning("Your health left:" + enemyAttack.player.health);

            StartCoroutine(Rumble.RumblePulse(0.25f, 1f, 0.5f));
            attacking = false;
            StartCoroutine(enemyAttack.AttackCoolDown());
        }

        //StartCoroutine(EndAttack());
    }

    /*IEnumerator EndAttack()
    {
        yield return new WaitForSeconds(0.625f);

        
    }*/
}
