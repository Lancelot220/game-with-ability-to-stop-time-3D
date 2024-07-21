using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class Attack1 : MonoBehaviour
{
    //Attack
    public int attackPower = 10;
    public bool attacking;
    public float comboTime = 0.5f;
    public float attackMoveForce = 50;
    public float knockback = 300;
    public float knockbackY = 100;
    float comboTimeCounter;
    Transform playerTransform;
    int hitCounter;
    
    //Others
    public float rotationSpeed = 10.0f; // Швидкість обертання
    public float targetAngle = -90; // Кінцевий градус обертання

    Movement m;

    //Input
    Controls ctrls;
    InputAction attack;
    void Awake() { ctrls = new Controls(); m = GetComponentInParent<Movement>(); playerTransform = GetComponentInParent<Transform>();}
    void OnEnable()
    {
        attack = ctrls.Player.Attack;
        attack.Enable();
        attack.performed += Attack;    
    }
    void OnDisable() { attack.Disable(); }
    void Attack(InputAction.CallbackContext context)
    {
        if (!attacking && GetComponentInParent<PlayerStats>().health > 0)
        {
            attacking = true;
            m.animator.SetTrigger("isAttacking");
            hitCounter++; //finish crit damage
        }
    }
    void Update()
    {
        if (attacking)
        {
            m.rb.AddForce(playerTransform.forward * attackMoveForce);
            //m.rb.AddForce(Vector3.Cross(playerTransform.forward, Vector3.up) * attackMoveForce);

            // Обертання об'єкта
            transform.RotateAround(transform.parent.position, Vector3.up, rotationSpeed * Time.deltaTime);
            if (transform.localEulerAngles.y > 90 && transform.localEulerAngles.y <= 270)
            {
                // Миттєве повернення в початкову позицію
                transform.localEulerAngles = new Vector3(0, 90, 0);
                attacking = false;
                //m.animator.SetBool("isAttacking", false);
                comboTimeCounter = comboTime;
            }
            
        }
        else
        {
            comboTimeCounter -= Time.deltaTime;
            m.animator.SetFloat("combo", comboTimeCounter);
        }

    }

    void OnTriggerEnter(Collider col)
    {
        if(col.CompareTag("Enemy") && attacking)
        {
            //playerTransform.LookAt(new Vector3(col.gameObject.transform.position.x, playerTransform.position.y, col.gameObject.transform.position.z));
            Enemy enemy = col.gameObject.GetComponent<Enemy>();
            if(enemy != null)
            {
                if(enemy.health > 0 && !enemy.timeStopped)
                {
                    enemy.attacked = true;
                    Vector3 knockbackDir = transform.forward * knockback;
                    knockbackDir.y = knockbackY;
                    enemy.rb.AddForce(knockbackDir);
                    enemy.health -= attackPower;
                    print("Enemy's health left:" + enemy.health);
                }
            }
            else
            {
                EnemyWithGun enemyWithGun = col.gameObject.GetComponent<EnemyWithGun>();
                if(enemyWithGun.health > 0 && !enemyWithGun.timeStopped)
                {
                    enemyWithGun.attacked = true;
                    Vector3 knockbackDir = transform.forward * knockback;
                    knockbackDir.y = knockbackY;
                    enemyWithGun.rb.AddForce(knockbackDir);
                    enemyWithGun.health -= attackPower;
                    print("Enemy's health left:" + enemyWithGun.health);
                }
            }
        }
    }
}
