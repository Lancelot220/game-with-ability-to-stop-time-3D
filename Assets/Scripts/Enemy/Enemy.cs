using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

//using System.Numerics;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor.AI;
using UnityEditor.Profiling.Memory.Experimental;

//using System.Numerics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public int health = 50;
    private bool deathMessageSent = false;
    PlayerStats ps;

    [Header("Moving")]
    public float startWaitTime = 4;
    public float timeToRotate = 2;
    public float speedWalk = 6;
    public float speedRun = 9;
    [HideInInspector] public Rigidbody rb;

    [Header("Enemy AI Options")]
    public NavMeshAgent navMeshAgent;
    public float viewRadius = 15;
    public float viewAngle = 90;
    public LayerMask playerMask;
    public LayerMask obstacleMask;
    public float catchingRadius = 2.5f;

    public Transform[] waypoints;
    int m_CurrentWayPointIndex;

    Vector3 playerLastPosition = Vector3.zero;
    Vector3 m_PlayerPosition;

    float m_WaitTime, m_TimeToRotate;
    bool m_PlayerInRange, m_PlayerNear, m_IsPatrol;
    [HideInInspector] public bool _CaughtPlayer;
    //stop moving while being attacking
    [HideInInspector] public bool attacked;
    
    //Stop time feature
    [Header("Others")]
    public bool timeStopped;
    public GameObject hpBoost;
    public int hpBoostDropChance = 2;
    Animator animator;

    void Start()
    {
        Shuffle(waypoints);

        m_PlayerPosition = Vector3.zero;
        m_IsPatrol = true;
        _CaughtPlayer = false;
        m_PlayerInRange = false;
        m_WaitTime = startWaitTime;
        m_TimeToRotate = timeToRotate;

        m_CurrentWayPointIndex = 0;
        navMeshAgent = GetComponent<NavMeshAgent>();
        ps = GameObject.Find("Player").GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        navMeshAgent.isStopped = false;
        navMeshAgent.speed = speedWalk;
        navMeshAgent.SetDestination(waypoints[m_CurrentWayPointIndex].position); //animator.SetBool("IsMoving", true);
    }
    //shufle waypoints (cuz i don't want to do it manually)
    void Shuffle<T>(T[] array)
{
    Array.Sort(array, (a, b) => UnityEngine.Random.Range(-1, 1));
}


    void Update()
    {
        if (!timeStopped && !attacked && !_CaughtPlayer)
        {
            EnvironmentView();

            if(!m_IsPatrol && ps.health > 0)
            {
                Chasing();
            }
            else
            {
                Patroling();
            }
        }
        
        _CaughtPlayer = Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) <= catchingRadius;  //

        if(attacked) 
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            StartCoroutine(DisableAttacked());
        }

        if(rb.velocity == Vector3.zero)
        rb.constraints = RigidbodyConstraints.FreezeAll;

        //Death
        if (health <= 0 && !deathMessageSent)
        {
            Debug.Log("The enemy was killed");
            deathMessageSent = true;
            System.Random rnd = new();
            if (rnd.Next(1, hpBoostDropChance) == 1) {Instantiate(hpBoost, transform.position, transform.rotation); print("HP Boost was dropped!");}
            Destroy(gameObject);
        }

        //Animations
        if (!timeStopped)
        {
            if(navMeshAgent.speed > 0 && navMeshAgent.speed < speedRun)
            {
                animator.SetBool("IsMoving", true);
                animator.SetBool("IsRunning", false);
            }
            else if (navMeshAgent.speed > speedWalk)
            {
                animator.SetBool("IsMoving", true);
                animator.SetBool("IsRunning", true);
            }
            else if (navMeshAgent.speed == 0)
            {
                animator.SetBool("IsMoving", false);
                animator.SetBool("IsRunning", false);
            }
        }
    }

    IEnumerator DisableAttacked()
    {
        yield return new WaitForSeconds(1);
        attacked = false;
    }

    void Chasing()
    {
        m_PlayerNear = false;
        playerLastPosition = Vector3.zero;

        if(!_CaughtPlayer)
        {
            Move(speedRun); //animator.SetBool("IsRunning", true);
            navMeshAgent.SetDestination(m_PlayerPosition);
        }
        if(navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            if(m_WaitTime <= 0 && !_CaughtPlayer && Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) >= 6f)
            {
                m_IsPatrol = true;
                m_PlayerNear = false;
                Move(speedWalk); //animator.SetBool("IsRunning", false);
                m_TimeToRotate = timeToRotate;
                m_WaitTime = startWaitTime;
                navMeshAgent.SetDestination(waypoints[m_CurrentWayPointIndex].position);
            }
            else
            {
                if(Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) <= catchingRadius /*2.5f*/)    //there was > instead of <=
                {
                    Stop();
                    m_WaitTime -= Time.deltaTime;
                    CaughtPlayer(); //
                }
            }
        }
    }
    void Patroling()
    {
        if (m_PlayerNear)
        {
            if(m_TimeToRotate <= 0)
            {
                Move(speedWalk); //animator.SetBool("IsRunning", false);
                LookingPlayer(playerLastPosition);
            }
            else
            {
                Stop();
                m_TimeToRotate -= Time.deltaTime;
            }
        }
        else
        {
            m_PlayerNear = false;
            playerLastPosition = Vector3.zero;
            navMeshAgent.SetDestination(waypoints[m_CurrentWayPointIndex].position);
            if(navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                if(m_WaitTime <= 0)
                {
                    NextPoint();
                    Move(speedWalk); //animator.SetBool("IsRunning", false);
                    m_WaitTime = startWaitTime;
                }
                else
                {
                    Stop();
                    m_WaitTime -= Time.deltaTime;
                }
            }
        }
    }
    void Move(float speed)
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = speed;
        //animator.SetBool("IsMoving", true);
    }
    void Stop()
    {
        navMeshAgent.isStopped = true;
        navMeshAgent.speed = 0;
        //animator.SetBool("IsMoving", false);
        //animator.SetBool("IsRunning", false);
    }
    public void NextPoint()
    {
        m_CurrentWayPointIndex = (m_CurrentWayPointIndex + 1) % waypoints.Length;
        navMeshAgent.SetDestination(waypoints[m_CurrentWayPointIndex].position);
        //animator.SetBool("IsMoving", true);
    }
    void CaughtPlayer() { _CaughtPlayer = true; }
    
    void LookingPlayer(Vector3 player)
    {
        navMeshAgent.SetDestination(player);
        if(Vector3.Distance(transform.position, player) <= 0.3)
        {
            if(m_WaitTime <= 0)
            {
                m_PlayerNear = false;
                Move(speedWalk); //animator.SetBool("IsRunning", false);
                navMeshAgent.SetDestination(waypoints[m_CurrentWayPointIndex].position);
                m_WaitTime = startWaitTime;
                m_TimeToRotate = timeToRotate;
            }
            else
            {
                Stop();
                m_WaitTime -= Time.deltaTime;
            }
        }
    }

    void EnvironmentView()
    {
        Collider[] playerInRange = Physics.OverlapSphere(transform.position, viewRadius, playerMask);

        for(int i = 0; i < playerInRange.Length; i++)
        {
            Transform player = playerInRange[i].transform;
            Vector3 dirToplayer = (player.position - transform.position).normalized;
            if(Vector3.Angle(transform.forward, dirToplayer) < viewAngle / 2)
            {
                float dstToPlayer = Vector3.Distance(transform.position, player.position);
                if(!Physics.Raycast(transform.position, dirToplayer, dstToPlayer, obstacleMask))
                {
                    m_PlayerInRange = true;
                    m_IsPatrol = false;
                }
                else
                {
                    m_PlayerInRange = false;
                }
            }
            if(Vector3.Distance(transform.position, player.position) > viewRadius)
            {
                m_PlayerInRange = false;
            }
            if(m_PlayerInRange)
            {
                m_PlayerPosition = player.transform.position;
            }
        }
        
    }
/*
    void OnCollisionEnter(Collision col)
    {
        if(col.collider.CompareTag("Player"))
        {
            PlayerStats player = col.gameObject.GetComponent<PlayerStats>();

            if (player.health > 0 && !timeStopped)
            {
                EnemyAttack enemyAttack = GetComponentInChildren<EnemyAttack>();
                player.health -=enemyAttack.attackPower;
                Debug.LogWarning("Your health left:" + player.health);
            }
        }
    }
*/
}