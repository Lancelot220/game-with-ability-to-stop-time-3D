using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyGunAttack : MonoBehaviour
{
    //Attack
    public float attackCD = 3;
    public bool isRecoiling;
    public float recoilDuration = 5;
    public LayerMask obstacleMask;
    private bool playerInRange;
    Animator playerAnimator;
    private Shooting shooting;
    Transform player;
    
    EnemyWithGun enemy;
    //Stop time feature
    public bool timeStopped;

    void Start() { enemy = GetComponentInParent<EnemyWithGun>(); shooting = GetComponent<Shooting>(); player = GameObject.FindGameObjectWithTag("Player").transform;}
    //Recoil when needed
    void Update()
    {
        if(shooting.ammo <= 0 && !isRecoiling)
        {
            isRecoiling = true;
            StartCoroutine(Recoil());
        }

        //aim
        if(playerInRange && !timeStopped) transform.LookAt(player);
    }

    IEnumerator Recoil()
    {
        yield return new WaitForSeconds(recoilDuration);

        shooting.ammo = shooting.maxAmmo;
        isRecoiling = false;
    }

    //Should attack
    void OnTriggerEnter(Collider col)
    {
        if(col.CompareTag("Player"))
        {
            playerInRange = true;
            enemy._CaughtPlayer  = true;
            playerAnimator = col.gameObject.GetComponentInChildren<Animator>();

            StartCoroutine(AttackCoolDown());
        }
    }

    //Attack
    IEnumerator AttackCoolDown()
    {
        yield return new WaitForSeconds(attackCD);

        if (playerInRange && 
        !playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack1") && !playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack2") &&
        !enemy.attacked && !timeStopped) 
            Attack();
    }

    void Attack()
    {
        bool isSeeingObstacle = Physics.Raycast(transform.position, transform.forward, Vector3.Distance(transform.position, player.gameObject.transform.position), obstacleMask); 
        if (/*player.health > 0 &&*/ !timeStopped && !isRecoiling && !isSeeingObstacle)
        {
            shooting.Shoot();
            //Debug.LogWarning("*pulling the trigger*");
        }

        StartCoroutine(AttackCoolDown());
    }

    //Shouldn't attack
    void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Player")) { playerInRange = false; enemy._CaughtPlayer = false; }
    }
}
