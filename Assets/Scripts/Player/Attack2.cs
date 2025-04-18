using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
//using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Attack2 : MonoBehaviour
{
    [Header("Attack")]
    public int attackPower;
    public int defaultPower = 10;
    public int critMultipier = 3;
    int critPower;
    public bool attacking;
    public float attackMoveForce = 50;

    [Header("Combo")]
    public float comboTime = 0.2f;
    float comboTimeCounter;

    [Header("Knockback")]
    public float knockback = 10;
    public float knockbackY = 5;
    public float knockbackTime = 5;
    
    [Header("Tricks")]
    public bool allow360 = true;
    public bool allowJumpWith360 = true;
    public bool allowFrontflipattack = true;
    public bool allowBackflip = true;

    //Others
    //public float attackTime  = 1f;
    Movement m;
    Transform playerTransform;
    TrailRenderer[] trails;
   bool jumpedWith360;
   bool attackEndedByFalling;

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
        if (!attacking && GetComponentInParent<PlayerStats>().health > 0 && !m.gameObject.GetComponent<PlayerStats>().pauseMenu.activeSelf)
        {
            attacking = true;
            m.animator.SetTrigger("isAttacking"); //Debug.Log($"onGround: {m.onGround}, 360: {m.animator.GetBool("360")}, jumpedWith360: {jumpedWith360}");
            if (m.onGround && !m.animator.GetBool("360")) // DEFAULT SWING
            {
                m.rb.velocity = Vector3.zero;
                //m.rb.velocity = playerTransform.forward * attackMoveForce;
                //StartCoroutine(StopDash());
                
                m.rb.AddForce(playerTransform.forward * attackMoveForce * 0.5f, ForceMode.Impulse);
            }
            else if (m.animator.GetBool("360") && allowJumpWith360 && !m.onGround && !jumpedWith360) //JUMP WITH 360
            {
                //m.rb.velocity = new Vector3(m.rb.velocity.x, attackMoveForce, m.rb.velocity.z);

                m.rb.AddForce(playerTransform.up * attackMoveForce, ForceMode.Impulse);
                jumpedWith360 = true;
                //StartCoroutine(StopDash());
            }

            //Trail
            trails = GetComponentsInChildren<TrailRenderer>();
            foreach( TrailRenderer trail in trails )
            {
                trail.enabled = true ;
            }

            StartCoroutine(Rumble.RumblePulse(0.25f, 1f, 0.25f));

            attackEndedByFalling = false;
        }
    }

    public void AttackEnd()
    {
        comboTimeCounter = comboTime;
        attacking = false;
        m.animator.ResetTrigger("isAttacking");
        
        if(m.onGround && !m.animator.GetBool("360")) m.rb.velocity = Vector3.zero;

        if(trails != null)
        {
            foreach( TrailRenderer trail in trails )
            {
                trail.enabled = false ;
            }
        }
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
            {
                attackPower = critPower;
                if(m.onGround) AttackEnd(); //increases attack power and ends attack through code because animation doesn't have AttackEnd event because it has to wait for player to land
            }
            else if (m.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Falling" || 
            m.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "LandingHard" || 
            m.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "GetUp")
            {
                if(!attackEndedByFalling) { AttackEnd(); attackEndedByFalling = true; }
            }
        }
        else attackPower = defaultPower;

        //360 attack
        m.animator.SetBool("360", do360.ReadValue<Vector2>().x > 0 && do360.ReadValue<Vector2>().y > 0 && allow360);
        if(m.onGround) jumpedWith360 = false;

        if(!attacking) m.animator.gameObject.transform.localEulerAngles = Vector2.zero;
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
