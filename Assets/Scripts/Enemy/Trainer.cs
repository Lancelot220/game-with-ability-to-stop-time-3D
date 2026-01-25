using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trainer : Enemy
{
    Attack2 attack;
    public bool isFighting;
    public int hitsThreshold = 3;
    public float attackRange = 2f;
    public int myHits;
    private float evadeTimer = 0f;
    private float blockTimer = 0f;
    private bool isBlocking = false;
    public enum State { Idle, Approach, Attack, Evade, Block, SpecialAttack }
    public State currentState = State.Idle;
    Footsteps footsteps;

    protected override void Start_()
    {
        shuffleWaypoints = false;
        attack = GetComponentInChildren<Attack2>();
        attack.rb = GetComponent<Rigidbody>();
        attack.animator = GetComponentInChildren<Animator>();
        attack.playerTransform = transform;
        footsteps = GetComponentInChildren<Footsteps>();
    }

    override protected void EnemyLogic()
    {
        if (isFighting && ps.health > 0)
        {
            UpdateTimers();

            // Determine state based on conditions
            if (hits >= hitsThreshold && health > lowHPThreshold)
            {
                // If hit too many times, evade or block
                if (Random.value > 0.5f)
                    currentState = State.Evade;
                else
                    currentState = State.Block;
            }
            else if (health <= lowHPThreshold)
            {
                // Low health, prioritize evasion
                currentState = State.Evade;
            }
            else if (Vector3.Distance(transform.position, ps.transform.position) < attackRange && navMeshAgent.isOnNavMesh)
            {
                // In attack range, decide to attack or special
                if (Random.value > 0.8f) // 20% chance for special attack
                    currentState = State.SpecialAttack;
                else
                    currentState = State.Attack;
            }
            else
            {
                // Approach if not in range and not evading
                currentState = State.Approach;
            }

            // Execute state
            switch (currentState)
            {
                case State.Approach:
                    Block(false); Frontflip(false); Backflip(false);
                    Approach(); 
                    break;
                case State.Attack:
                    Block(false); Frontflip(false); Backflip(false);
                    Attack(); 
                    break;
                case State.Evade:
                    Block(false); Frontflip(false); Backflip(false);
                    Evade(); 
                    break;
                case State.Block:
                    Frontflip(false); Backflip(false);
                    Block(true); 
                    break;
                case State.SpecialAttack:
                    Block(false); Frontflip(false); Backflip(false);
                    SpecialAttack(); 
                    break;
                default:
                    Block(false); Frontflip(false); Backflip(false);
                    Idle();
                    break;
            }
        }
        else
        {
            Idle();
            currentState = State.Idle;
        }

        animator.SetBool("onGround", navMeshAgent.isOnNavMesh);
        footsteps.onGround = navMeshAgent.isOnNavMesh;

        if(health <= 0)
        {
            Debug.LogWarning("Trainer died");
            gameObject.SetActive(false);
        }

        if(animator.GetCurrentAnimatorClipInfo(0).Length > 0 && 
        animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Backflip" &&
        animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "FrontflipAttack")
        navMeshAgent.enabled = true;
    }

    private void UpdateTimers()
    {
        if (evadeTimer > 0) evadeTimer -= Time.deltaTime;
        if (blockTimer > 0) blockTimer -= Time.deltaTime;

        // Reset states after timers
        if (evadeTimer <= 0 && currentState == State.Evade)
        {
            currentState = State.Approach;
        }
        if (blockTimer <= 0 && currentState == State.Block)
        {
            Block(false);
            currentState = State.Approach;
        }
    }

    void Idle()
    {
        Stop();
    }

    void Approach()
    {
        Move(speedRun);
        navMeshAgent.SetDestination(ps.transform.position);
    }

    void Evade()
    {
        if (evadeTimer <= 0)
        {
            evadeTimer = Random.Range(2f, 5f); // Evade for 2-5 seconds
            // Chance to use backflip for stun and retreat
            if (Random.value > 0.7f) // 30% chance to use backflip evade
            {
                StartCoroutine(PerformSpecialAttack(false, true)); // Stuns the player and retreats
            }
            else
            {
                // Move away from player
                Vector3 directionAway = (transform.position - ps.transform.position).normalized;
                Vector3 evadePosition = transform.position + directionAway * 5f;
                navMeshAgent.SetDestination(evadePosition);
                Move(speedRun);
            }
        }
    }

    void Attack()
    {
        Stop();
        attack.Attack();
        myHits++;
        // After attack, reset hits or something, but for now, just attack
    }

    void Block(bool on)
    {
        if (on && !isBlocking)
        {
            isBlocking = true;
            blockTimer = Random.Range(minBlockTime, maxBlockTime);
            attack.block = 1;
            Stop();
        }
        else if (!on && isBlocking)
        {
            isBlocking = false;
            attack.block = 0;
            Move(speedWalk);
        }
    }

    void SpecialAttack()
    {
        // Randomly choose special attack
        if (Random.value > 0.5f)
        {
            StartCoroutine(PerformSpecialAttack(true, false)); // Frontflip
        }
        else
        {
            StartCoroutine(PerformSpecialAttack(false, true)); // Backflip
        }
    }

    private IEnumerator PerformSpecialAttack(bool isFrontflip, bool isBackflip)
    {
        // Activate the special move (simulate holding keys)
        if (isFrontflip)
        {
            Frontflip(true);
        }
        else if (isBackflip)
        {
            Backflip(true);
        }

        // Wait for crouch/charge time (visible delay)
        yield return new WaitForSeconds(0.5f); // Adjust time as needed for crouch animation

        // Perform the attack
        attack.Attack();
        myHits++;
        navMeshAgent.enabled = false; // Disable NavMeshAgent during the special attack

        // Release the keys after attack
        if (isFrontflip)
        {
            Frontflip(false);
        }
        else if (isBackflip)
        {
            Backflip(false);
        }
    }

    void Frontflip(bool activate)
    {
        if (activate)
        {
            attack.frontflipAttack = new Vector2(1, 1);
            animator.SetBool("isCrouching", true);
        }
        else
        {
            attack.frontflipAttack = Vector2.zero;
            animator.SetBool("isCrouching", false);
        }
    }

    void Backflip(bool activate)
    {
        if (activate)
        {
            attack.backflip = new Vector2(1, 1);
            animator.SetBool("isCrouching", true);
        }
        else
        {
            attack.backflip = Vector2.zero;
            animator.SetBool("isCrouching", false);
        }
    }

    void WaitForPlayer()
    {
        // Optional: wait or circle around
    }
}
