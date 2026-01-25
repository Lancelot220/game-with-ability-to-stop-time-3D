using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEditor;
//using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Attack2 : MonoBehaviour
{
    [Header("Attack")]
    public int attackPower;
    public int defaultPower = 10;
    public int jaMultipier = 3; //ja - Jump Attack
    int ffaPower;
    int jaPower;
    public bool attacking;
    public float attackMoveForce = 50;

    [Header("Combo")]
    public float comboTime = 0.2f;
    float comboTimeCounter;

    [Header("Knockback")]
    public float knockback = 10;
    public float knockbackY = 5;
    public float knockbackTime = 5;
    
    [Header("Skills")]
    public float skillsCooldown = 5;
    public int ffaMultipier = 5; //ffa - FrontFlip Attack
    public float ffaSwordThickness = 0.02f;
    float defaultSwordThickness;
    public float stunTime = 3;
    
    public bool allow360 = true;
    public bool allowJumpWith360 = true;
    public bool allowFrontflipattack = true;
    public bool allowBackflip = true;
    //public bool allowRoll = true;

    //Others
    //public float attackTime  = 1f;
    TrailRenderer[] trails;
   bool jumpedWith360;
   bool attackEnded;
   [SerializeField] float skillsCDTimer;
   public Slider skillsCD;
   [SerializeField] bool skillUsed;

    public float block;
    public Vector2 do360;
    public Vector2 frontflipAttack;
    public Vector2 backflip;

    [Header("Aim mode")]
    public CinemachineFreeLook mainCam;
    public CinemachineFreeLook aimCam;

    LineRenderer line;
    Vector3 trLocalPos;
    [HideInInspector] public Movement m;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Animator animator;
    [HideInInspector] public bool onGround;
    [HideInInspector] public float defaultSpeed;
    [HideInInspector] public float jumpForce;
    [HideInInspector] public Transform playerTransform;
    
    void Start()
    {
        jaPower = defaultPower * jaMultipier;
        // skillsCD = GameObject.Find("TricksCD").GetComponent<Slider>();
        // skillsCD.gameObject.SetActive(false);
        ffaPower = defaultPower * ffaMultipier;

        defaultSwordThickness = GetComponent<BoxCollider>().size.z;
    }
    public void LateStart()
    {
        line = m.GetComponentInChildren<LineRenderer>(true);
        line.enabled = false;
        trLocalPos = line.transform.localPosition;

        //skillsCD = GameObject.Find("TricksCD").GetComponent<Slider>();
        skillsCD.gameObject.SetActive(false);

        // GameObject mc = GameObject.Find("FreeLook Camera");
        // print(mc);
        // mainCam = mc.GetComponent<CinemachineFreeLook>();
        // print(mainCam);
        // GameObject ac = GameObject.Find("Aim Camera");
        // print(ac);
        // aimCam = ac.GetComponent<CinemachineFreeLook>();
        // print(aimCam);
    }
    
    public void Attack()
    {
        if (!attacking && GetComponentInParent<Damageable>().health > 0 && Time.timeScale != 0)
        {
            attacking = true;
            animator.SetTrigger("attacked");
            //MOVE PLAYER WHEN ATTACKING
            if (onGround && !animator.GetBool("360") && !animator.GetBool("frontflipAttack") && !animator.GetBool("backflip")) // DEFAULT SWING
            {
                rb.velocity = Vector3.zero;
                //rb.velocity = playerTransform.forward * attackMoveForce;
                //StartCoroutine(StopDash());
                
                rb.AddForce(playerTransform.forward * attackMoveForce * 0.5f, ForceMode.Impulse);
            }
            else if (animator.GetBool("360") && allowJumpWith360 && !onGround && !jumpedWith360) //JUMP WITH 360
            {
                //rb.velocity = new Vector3(rb.velocity.x, attackMoveForce, rb.velocity.z);

                rb.AddForce(playerTransform.up * attackMoveForce, ForceMode.Impulse);
                jumpedWith360 = true;
                //StartCoroutine(StopDash());
            }
            else if (animator.GetBool("frontflipAttack") && allowFrontflipattack && onGround) //FRONTFLIP ATTACK
            {
                if(m != null) m.CrouchStop_();
                rb.velocity = playerTransform.forward * defaultSpeed;
                rb.AddForce(new Vector3(rb.velocity.x, jumpForce, rb.velocity.z));
                line.transform.SetParent(null);
                GetComponent<BoxCollider>().size = new Vector3(GetComponent<BoxCollider>().size.x, GetComponent<BoxCollider>().size.y, ffaSwordThickness);
            }
            else if (animator.GetBool("backflip") && allowBackflip && onGround) //BACKFLIP
            {
                if(m != null) m.CrouchStop_();
                rb.velocity = playerTransform.forward * -defaultSpeed;
                rb.AddForce(new Vector3(rb.velocity.x, jumpForce, rb.velocity.z));
            }

            //Trail
            trails = GetComponentsInChildren<TrailRenderer>();
            foreach( TrailRenderer trail in trails )
            {
                trail.enabled = true ;
            }

            if (m != null) StartCoroutine(Rumble.RumblePulse(0.25f, 1f, 0.25f));

            attackEnded = false;

            if (recording) trajectoryPoints = new List<Vector3>();
        }
    }

    public void AttackEnd()
    {
        comboTimeCounter = comboTime;
        attacking = false;
        animator.ResetTrigger("attacked");
        
        if(onGround && !animator.GetBool("360")) rb.velocity = Vector3.zero;

        if(trails != null)
        {
            foreach( TrailRenderer trail in trails )
            {
                trail.enabled = false ;
            }
        }

        //reset skills cooldown timer if player used a skill
        if (skillUsed)
        {
            skillsCDTimer = skillsCooldown;
            skillUsed = false;
        }

        if(m != null) 
        {
            line.transform.SetParent(m.transform);
            line.transform.localRotation = Quaternion.identity;
            line.transform.localPosition = trLocalPos;
        }

        GetComponent<BoxCollider>().size = new Vector3(GetComponent<BoxCollider>().size.x, GetComponent<BoxCollider>().size.y, defaultSwordThickness);
    }
    Vector3? lastPlayerPos;
    public List<Vector3> trajectoryPoints;
    public bool recording;
    void Update()
    {
        // allow360 = GetComponentInParent<PlayerStats>().unlockedSkills.Contains("360");
        // allowJumpWith360 = GetComponentInParent<PlayerStats>().unlockedSkills.Contains("JumpWith360");
        // allowFrontflipattack = GetComponentInParent<PlayerStats>().unlockedSkills.Contains("FrontflipAttack");
        // allowBackflip = GetComponentInParent<PlayerStats>().unlockedSkills.Contains("Backflip");
        // //if (attacking && onGround) 

        if (!attacking)
        {
            if (comboTimeCounter > -1) comboTimeCounter -= Time.deltaTime;
            animator.SetFloat("combo", comboTimeCounter);
            lastPlayerPos = null;
        }
        else if(recording)
        {
            if (lastPlayerPos == null) lastPlayerPos = playerTransform.position;
            Debug.DrawLine((Vector3)lastPlayerPos, playerTransform.position, Color.red, 60);
            lastPlayerPos = playerTransform.position;
            trajectoryPoints.Add(playerTransform.position - line.transform.position);
        }

        //block
        if(!attacking && Time.timeScale != 0) animator.SetBool("block", block > 0);
        //360 attack
        animator.SetBool("360", do360.x > 0 && do360.y > 0 && allow360 && skillsCDTimer <= 0);
        if (onGround) jumpedWith360 = false;
        //Frontflip attack
        bool ffaPressed = frontflipAttack.x > 0 && frontflipAttack.y > 0 && allowFrontflipattack && skillsCDTimer <= 0;
        animator.SetBool("frontflipAttack", ffaPressed);
        if (ffaPressed && m != null) ShowTrajectory();
        else if (line != null) line.enabled = false;
        if (m != null) Aim(ffaPressed);

        //Backflip
        animator.SetBool("backflip", backflip.x > 0 && backflip.y > 0 && allowBackflip && skillsCDTimer <= 0);

        if (animator.GetCurrentAnimatorClipInfo(0).Length > 0)
        {
            if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Jump Attack")
            {
                attackPower = jaPower;
                // skillUsed = true;
                if (onGround) AttackEnd(); //increases attack power and ends attack through code because animation doesn't have AttackEnd event because it has to wait for player to land
            }
            else if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Falling" ||
            animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "LandingHard" ||
            animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "GetUp" ||
            animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Ledge climb" ||
            animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "InAir")
            {
                if (!attackEnded) { AttackEnd(); attackEnded = true; }
            }
            else if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "FrontflipAttack")
            {
                attackPower = ffaPower;
                skillUsed = true;
                if (onGround) AttackEnd();
            }
            else if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Actual360"|| animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Backflip") skillUsed = true;
            else if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Attack1" ||
            animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Attack2") attackPower = defaultPower;
        }

        if(!attacking) animator.gameObject.transform.localEulerAngles = Vector2.zero;

        //skills cooldown
        if(skillsCDTimer > -1) skillsCDTimer -= Time.deltaTime;
        if (skillsCD != null)
        {
            if (skillsCDTimer > 0) skillsCD.gameObject.SetActive(true);
            else skillsCD.gameObject.SetActive(false);
            skillsCD.value = skillsCooldown - skillsCDTimer;
        }

        //if (animator.GetCurrentAnimatorClipInfo(0).Length > 0 && animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "FrontflipAttack") m.speed = defaultSpeed;
    }

    // public void Roll(float x)
    // {
    //     if(allowRoll && skillsCDTimer <= 0 && x != 0 && skill.ReadValue<float>() > 0 && onGround)
    //     {
    //         animator.SetTrigger("roll");
    //         //animator.SetBool("isCrouching", false);
    //         if(x > 0) { x = 1; animator.SetBool("rollDir", true); }
    //         else { x = -1; animator.SetBool("rollDir", false); }
    //         rb.AddForce(playerTransform.right * x * attackMoveForce, ForceMode.Impulse);
    //         skillsCDTimer = 1f;
    //     }
    // }

    void OnTriggerEnter(Collider col)
    {
        if (/*col.CompareTag("Enemy") && */attacking)
        {
            Damageable enemy = col.gameObject.GetComponent<Damageable>();
            if (enemy != null)
            {
                Transform blocker = col.transform;      // той, хто блокує
                Transform attacker = transform;         // той, хто атакує

                Vector3 toAttacker = (attacker.position - blocker.position).normalized;

                // наскільки атакуючий "попереду"
                float dot = Vector3.Dot(blocker.forward, toAttacker);

                if (col.GetComponentInChildren<Animator>().GetCurrentAnimatorClipInfo(0).Length > 0 &&                                 //check for blocking
                col.GetComponentInChildren<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name == "Block" &&
                //Physics.Raycast(col.transform.position, col.transform.forward, out RaycastHit hit, 3f, 1 << 3) &&                     //check for facing to player
                //hit.collider.transform.parent.GetComponentInChildren<Attack2>() == this)                                                //check is the enemy the ray hits is this player (do i need this?)
                dot > 0.5f)
                {
                    Debug.Log(col.gameObject.name + " block hit!");                                                                                          //if the enemy is blocking, do nothing
                    return;
                }

                
                if (enemy.health > 0)
                {
                    // if (enemy.TryGetComponent(out Enemy e) && e.timeStopped)
                    // return;

                    //enemy.attacked = true;
                    Vector3 knockbackDir = playerTransform.forward * knockback;
                    knockbackDir.y = Mathf.Abs(knockbackY);
                    NavMeshAgent nma = enemy.gameObject.GetComponent<NavMeshAgent>();
                    if(nma != null) nma.enabled = false;

                    enemy.GetComponent<Rigidbody>().AddForce(knockbackDir, ForceMode.Impulse);
                    //StartCoroutine(EnableNavMesh(enemy.gameObject /*, enemy.rb*/ ));
                    enemy.health -= attackPower;
                    enemy.hits++;
                    print(enemy.gameObject.name + "'s health left:" + enemy.health);
                    if(animator.GetCurrentAnimatorClipInfo(0).Length > 0 && animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Backflip")
                    {
                        enemy.stunned = stunTime;
                        print("Enemy stunned for " + stunTime + " seconds");
                    }
                }
            }
            // else
            // {
            //     EnemyWithGun enemyWithGun = col.gameObject.GetComponent<EnemyWithGun>();
            //     if(enemyWithGun.health > 0 && !enemyWithGun.timeStopped)
            //     {
            //         //enemyWithGun.attacked = true;
            //         Vector3 knockbackDir = playerTransform.forward * knockback;
            //         knockbackDir.y = Mathf.Abs(knockbackY);
            //         enemyWithGun.gameObject.GetComponent<NavMeshAgent>().enabled = false;
            //         enemyWithGun.rb.AddForce(knockbackDir, ForceMode.Impulse);
            //         //StartCoroutine(EnableNavMesh(enemyWithGun.gameObject /*, enemyWithGun.rb */));
            //         enemyWithGun.health -= attackPower;
            //         print("Enemy's health left:" + enemyWithGun.health);
            //     }
            // }
        }
        else if (col.CompareTag("Breakable") & attacking)
        {
            col.gameObject.GetComponent<Breakables>().Break();
        }
    }

    // IEnumerator EnableNavMesh(GameObject hitEnemy /*,Rigidbody hitEnemyRb*/ )
    // {
    //     yield return new WaitForSeconds(knockbackTime);
    //     if(hitEnemy != null)
    //     {
    //         //Until(() => hitEnemyRb.velocity.magnitude <= stopThreshold);

    //         hitEnemy.GetComponent<NavMeshAgent>().enabled = true;
    //     }
    //     //else yield return null;
    // }

    void ShowTrajectory()
    {
        line.enabled = true;

        // Vector3 startPos = line.transform.position; //transform.position;
        // Vector3 startVel = playerTransform.forward * defaultSpeed + Vector3.up * (jumpForce / rb.mass / 1000);

        // Vector3[] points = new Vector3[resolution];
        // points[0] = startPos;

        // for (int i = 1; i < resolution; i++)
        // {
        //     float t = i * timeStep;
        //     Vector3 point = startPos + startVel * t + 0.5f * Physics.gravity * t * t;

        //     // Перевірка на зіткнення
        //     if (Physics.Raycast(points[i - 1], point - points[i - 1], out RaycastHit hit, (point - points[i - 1]).magnitude, collisionMask))
        //     {
        //         points[i] = hit.point;
        //         line.positionCount = i + 1;
        //         line.SetPositions(points);
        //         return;
        //     }

        //     points[i] = point;
        // }

        // line.positionCount = resolution;
        // line.SetPositions(points);

        line.positionCount = trajectoryPoints.Count;
        for (int i = 0; i < trajectoryPoints.Count; i++)
        {
            Vector3 absPos = line.transform.TransformPoint(trajectoryPoints[i]); //trajectoryPoints[i] + line.transform.position;
            line.SetPosition(i, absPos);
        }
    }
    
    void Aim(bool aiming)
    {
        m.aimMode = aiming;
        if (aiming)
        {
            mainCam.Priority = 0;
            aimCam.Priority = 10;

            playerTransform.rotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);

            mainCam.m_XAxis.Value = aimCam.m_XAxis.Value;
        }
        else
        {
            mainCam.Priority = 10;
            aimCam.Priority = 0;
            aimCam.m_XAxis.Value = mainCam.m_XAxis.Value;
        }
    }
}
