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
        if (col.CompareTag("Player") && col.GetComponent<Movement>() && attacking && enemyAttack.player.health > 0 && !enemyAttack.timeStopped)
        {
            if(col.GetComponentInChildren<Animator>().GetCurrentAnimatorClipInfo(0).Length > 0 &&                                 //check for blocking
            col.GetComponentInChildren<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name == "Block" && 
            Physics.Raycast(col.transform.position, col.transform.forward, out RaycastHit hit, 3f, 1 << 7) &&                     //check for facing to enemy
            hit.collider.transform.parent.GetComponentInChildren<Sword>() == this)                                                //check is the enemy the ray hits is this enemy
            {
                StartCoroutine(enemyAttack.AttackCoolDown());
                Debug.Log("Block hit!");                                                                                          //if the player is blocking, do nothing
                return;
            }
            //Debug.DrawRay(col.transform.position, col.transform.forward * 3f, Color.red, 5f);                                    //debug ray to check if the player is facing the enemy
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
