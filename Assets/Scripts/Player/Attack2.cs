using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Attack2 : MonoBehaviour
{
    //Attack
    public int attackPower;
    public int defaultPower = 10;
    public int critMultipier = 3;
    int critPower;
    public bool attacking;
    public float attackMoveForce = 50;
     public float dashTime = 0.15f;
    //Combo
    public float comboTime = 0.2f;
    float comboTimeCounter;
    //int hitCounter;
    //Knockback
    public float knockback = 10;
    public float knockbackY = 5;
    public float stopThreshold = 0.01f;
    public float knockbackTime = 5;
    
    
    //Others
    //public float attackTime  = 1f;
    Movement m;
    Transform playerTransform;
    TrailRenderer[] trails;

    //Input
    Controls ctrls;
    InputAction attack;
    InputAction do360;

    void Awake() { ctrls = new Controls(); m = GetComponentInParent<Movement>(); playerTransform = m.gameObject.transform; }
    void Start() { critPower = attackPower * critMultipier;}
    void OnEnable()
    {
        attack = ctrls.Player.Attack;
        attack.Enable();
        attack.performed += Attack;

        do360 = ctrls.Player._360;
        do360.Enable();
    }
    void OnDisable() { attack.Disable();  do360.Disable(); }
    void Attack(InputAction.CallbackContext context)
    {
        if (!attacking && GetComponentInParent<PlayerStats>().health > 0 && !m.pauseMenu.activeSelf)
        {
            attacking = true;
            m.animator.SetTrigger("isAttacking");
            if (m.onGround && !m.animator.GetBool("360"))
            {
                m.rb.velocity = Vector3.zero;
                m.rb.velocity = playerTransform.forward * attackMoveForce;
                StartCoroutine(StopDash());
                //m.rb.AddForce(playerTransform.forward * attackMoveForce);
            }

            //Trail
            trails = GetComponentsInChildren<TrailRenderer>();
            foreach( TrailRenderer trail in trails )
            {
                trail.enabled = true ;
            }

            StartCoroutine(Rumble.RumblePulse(0.25f, 1f, 0.25f));
        }
    }

    public void AttackEnd()
    {
        comboTimeCounter = comboTime;
        attacking = false;
        m.animator.ResetTrigger("isAttacking");

        //if(m.onGround) m.rb.velocity = Vector3.zero;

        foreach( TrailRenderer trail in trails )
        {
            trail.enabled = false ;
        }
    }

    IEnumerator StopDash()
    {
        yield return new WaitForSeconds( dashTime );

        if(m.onGround) m.rb.velocity = Vector3.zero;
    }

    void Update()
    {
        //if (attacking && m.onGround) 

        if (!attacking)
        {
            /*if(comboTimeCounter > 0) */comboTimeCounter -= Time.deltaTime;
            m.animator.SetFloat("combo", comboTimeCounter);
        }
        if(m.animator.GetCurrentAnimatorClipInfo(0).Length > 0)
        {
            if (m.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Jump Attack") 
            { attackPower = critPower; }
        }
        else attackPower = defaultPower;

        //360 attack
        m.animator.SetBool("360", do360.ReadValue<Vector2>().x > 0 && do360.ReadValue<Vector2>().y > 0);
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.CompareTag("Enemy") && attacking)
        {
            Enemy enemy = col.gameObject.GetComponent<Enemy>();
            if(enemy != null)
            {
                if(enemy.health > 0 && !enemy.timeStopped)
                {
                    //enemy.attacked = true;
                    Vector3 knockbackDir = playerTransform.forward * knockback;
                    knockbackDir.y = Mathf.Abs(knockbackY);
                    enemy.gameObject.GetComponent<NavMeshAgent>().enabled = false;
                    enemy.rb.AddForce(knockbackDir, ForceMode.Impulse);
                    //StartCoroutine(EnableNavMesh(enemy.gameObject /*, enemy.rb*/ ));
                    enemy.health -= attackPower;
                    print("Enemy's health left:" + enemy.health);
                }
            }
            else
            {
                EnemyWithGun enemyWithGun = col.gameObject.GetComponent<EnemyWithGun>();
                if(enemyWithGun.health > 0 && !enemyWithGun.timeStopped)
                {
                    //enemyWithGun.attacked = true;
                    Vector3 knockbackDir = playerTransform.forward * knockback;
                    knockbackDir.y = Mathf.Abs(knockbackY);
                    enemyWithGun.gameObject.GetComponent<NavMeshAgent>().enabled = false;
                    enemyWithGun.rb.AddForce(knockbackDir, ForceMode.Impulse);
                    //StartCoroutine(EnableNavMesh(enemyWithGun.gameObject /*, enemyWithGun.rb */));
                    enemyWithGun.health -= attackPower;
                    print("Enemy's health left:" + enemyWithGun.health);
                }
            }
        }
        else if(col.CompareTag("Breakable") & attacking)
        {
            col.gameObject.GetComponent<Breakables>().Break();
        }
    }

    IEnumerator EnableNavMesh(GameObject hitEnemy /*,Rigidbody hitEnemyRb*/ )
    {
        yield return new WaitForSeconds(knockbackTime);
        if(hitEnemy != null)
        {
            //Until(() => hitEnemyRb.velocity.magnitude <= stopThreshold);

            hitEnemy.GetComponent<NavMeshAgent>().enabled = true;
        }
        //else yield return null;
    }
}
