using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StopTime_ : MonoBehaviour
{
    //options
    public float range = 50;
    public float duration = 30;
    public float cd = 60;
    private float durationTimer;
    private float cdTimer;
    public LayerMask stoppableObjects;
    public bool canStopTime = true;
    
    //stop time mechanic
    Collider[] objectsInRange;
    Vector3 navMeshAgentDst;

    //references
    Controls ctrls;
    InputAction stopTime;
    Slider slider;
    void Awake() { ctrls = new Controls(); slider = GameObject.Find("Stop Time").GetComponent<Slider>();}
    void OnEnable() { stopTime = ctrls.Player.StopTime; stopTime.Enable(); stopTime.performed += StopTime; }

    void Update()
    {
        if(durationTimer > 0 && !canStopTime)
        {durationTimer -= Time.deltaTime; slider.maxValue = duration; slider.value = durationTimer;}
        if(cdTimer < cd && !canStopTime && durationTimer <= 0)
        {cdTimer += Time.deltaTime; slider.maxValue = cd; slider.value = cdTimer;}
    }

    //stop time
    void StopTime(InputAction.CallbackContext context)
    {
        if(canStopTime)
        {
            canStopTime = false;

            print("Time is stopped! (English or Spanish?)");
            
            objectsInRange = Physics.OverlapSphere(transform.position, range, stoppableObjects);
            
            foreach (Collider obj in objectsInRange)
            {
                Rigidbody rb = obj.gameObject.GetComponent<Rigidbody>();
                Enemy enemy = obj.gameObject.GetComponent<Enemy>();
                EnemyWithGun enemyWithGun = obj.gameObject.GetComponent<EnemyWithGun>();
                NavMeshAgent navMeshAgent = obj.gameObject.GetComponent<NavMeshAgent>();
                if(rb != null)
                {
                    rb.constraints = RigidbodyConstraints.FreezeAll;
                    rb.useGravity = false;
                }
                if (enemy != null)
                {
                    navMeshAgentDst = navMeshAgent.destination;

                    enemy.timeStopped = true;
                    obj.gameObject.GetComponentInChildren<EnemyAttack>().timeStopped = true;

                    navMeshAgent.SetDestination(obj.transform.position);
                }
                else if (enemyWithGun != null)
                {
                    navMeshAgentDst = navMeshAgent.destination;

                    enemyWithGun.timeStopped = true;
                    obj.gameObject.GetComponentInChildren<EnemyGunAttack>().timeStopped = true;

                    navMeshAgent.SetDestination(obj.transform.position);
                }
            }

            StartCoroutine(UnfreezeTime());

            durationTimer = duration;
        }
    }
    IEnumerator UnfreezeTime()
    {
        yield return new WaitForSeconds(duration);

        foreach (Collider obj in objectsInRange)
        {
            Rigidbody rb = obj.gameObject.GetComponent<Rigidbody>();
            Enemy enemy = obj.gameObject.GetComponent<Enemy>();
            EnemyWithGun enemyWithGun = obj.gameObject.GetComponent<EnemyWithGun>();
            NavMeshAgent navMeshAgent = obj.gameObject.GetComponent<NavMeshAgent>();

            if(rb != null) rb.useGravity = true;
            if (enemy != null)
            {
                enemy.timeStopped = false;
                obj.gameObject.GetComponentInChildren<EnemyAttack>().timeStopped = false;

                navMeshAgent.SetDestination(navMeshAgentDst);
            }
            else if (enemyWithGun != null)
            {
                enemyWithGun.timeStopped = false;
                obj.gameObject.GetComponentInChildren<EnemyGunAttack>().timeStopped = false;

                navMeshAgent.SetDestination(navMeshAgentDst);
            }
            else { if(rb != null) rb.constraints = RigidbodyConstraints.None; }
        }

        objectsInRange = null;
        StartCoroutine(Cooldown());
        
        print("Time is unfreezed.");

        cdTimer = 0;
    }

    IEnumerator Cooldown()
    { yield return new WaitForSeconds(cd); canStopTime = true; print("You can stop time again!");}
}
