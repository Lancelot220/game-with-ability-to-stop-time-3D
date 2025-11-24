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
    public bool allow360 = true;
    public bool allowJumpWith360 = true;
    public bool allowFrontflipattack = true;
    public int ffaMultipier = 5; //ffa - FrontFlip Attack
    public float ffaSwordThickness = 0.02f;
    float defaultSwordThickness;
    public bool allowBackflip = true;
    public float stunTime = 3;
    //public bool allowRoll = true;

    //Others
    //public float attackTime  = 1f;
    Movement m;
    Transform playerTransform;
    TrailRenderer[] trails;
   bool jumpedWith360;
   bool attackEnded;
   [SerializeField] float skillsCDTimer;
   Slider skillsCD;
   [SerializeField] bool skillUsed;

    //Input
    Controls ctrls;
    InputAction attack;
    InputAction block;
    InputAction do360;
    InputAction frontflipAttack;
    InputAction backflip;
    InputAction skill;

    [Header("Aim mode")]
    public CinemachineFreeLook mainCam;
    public CinemachineFreeLook aimCam;

    LineRenderer line;
    Vector3 trLocalPos;
    void Awake() { ctrls = new Controls(); m = GetComponentInParent<Movement>(); playerTransform = m.gameObject.transform; }
    void Start()
    {
        jaPower = defaultPower * jaMultipier;
        skillsCD = GameObject.Find("TricksCD").GetComponent<Slider>();
        skillsCD.gameObject.SetActive(false);
        ffaPower = defaultPower * ffaMultipier;

        line = m.GetComponentInChildren<LineRenderer>();
        line.enabled = false;
        trLocalPos = line.transform.localPosition;

        mainCam = GameObject.Find("FreeLook Camera").GetComponent<CinemachineFreeLook>();
        aimCam = GameObject.Find("Aim Camera").GetComponent<CinemachineFreeLook>();

        defaultSwordThickness = GetComponent<BoxCollider>().size.z;
    }
    void OnEnable()
    {
        attack = ctrls.Player.Attack;
        attack.Enable();
        attack.performed += Attack;

        block = ctrls.Player.Block;
        block.Enable();

        do360 = ctrls.Player._360;
        do360.Enable();

        frontflipAttack = ctrls.Player.FrontflipAttack;
        frontflipAttack.Enable();

        backflip = ctrls.Player.BackFlip;
        backflip.Enable();

        skill = ctrls.Player.Skill;
        skill.Enable();
    }
    void OnDisable()
    { 
        attack.Disable();
        block.Disable();
        do360.Disable();
        frontflipAttack.Disable();
        backflip.Disable();
        skill.Disable();
    }
    void Attack(InputAction.CallbackContext context)
    {
        if (!attacking && GetComponentInParent<PlayerStats>().health > 0 && Time.timeScale != 0)
        {
            attacking = true;
            m.animator.SetTrigger("attacked");
            //MOVE PLAYER WHEN ATTACKING
            if (m.onGround && !m.animator.GetBool("360") && !m.animator.GetBool("frontflipAttack") && !m.animator.GetBool("backflip")) // DEFAULT SWING
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
            else if (m.animator.GetBool("frontflipAttack") && allowFrontflipattack && m.onGround) //FRONTFLIP ATTACK
            {
                m.CrouchStop_();
                m.rb.velocity = playerTransform.forward * m.defaultSpeed;
                m.rb.AddForce(new Vector3(m.rb.velocity.x, m.jumpForce, m.rb.velocity.z));
                line.transform.SetParent(null);
                GetComponent<BoxCollider>().size = new Vector3(GetComponent<BoxCollider>().size.x, GetComponent<BoxCollider>().size.y, ffaSwordThickness);
            }
            else if (m.animator.GetBool("backflip") && allowBackflip && m.onGround) //BACKFLIP
            {
                m.CrouchStop_();
                m.rb.velocity = playerTransform.forward * -m.defaultSpeed;
                m.rb.AddForce(new Vector3(m.rb.velocity.x, m.jumpForce, m.rb.velocity.z));
            }

            //Trail
            trails = GetComponentsInChildren<TrailRenderer>();
            foreach( TrailRenderer trail in trails )
            {
                trail.enabled = true ;
            }

            StartCoroutine(Rumble.RumblePulse(0.25f, 1f, 0.25f));

            attackEnded = false;

            if (recording) trajectoryPoints = new List<Vector3>();
        }
    }

    public void AttackEnd()
    {
        comboTimeCounter = comboTime;
        attacking = false;
        m.animator.ResetTrigger("attacked");
        
        if(m.onGround && !m.animator.GetBool("360")) m.rb.velocity = Vector3.zero;

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

        line.transform.SetParent(m.transform);
        line.transform.localRotation = Quaternion.identity;
        line.transform.localPosition = trLocalPos;

        GetComponent<BoxCollider>().size = new Vector3(GetComponent<BoxCollider>().size.x, GetComponent<BoxCollider>().size.y, defaultSwordThickness);
    }
    Vector3? lastPlayerPos;
    public List<Vector3> trajectoryPoints;
    public bool recording;
    void Update()
    {
        allow360 = GetComponentInParent<PlayerStats>().unlockedSkills.Contains("360");
        allowJumpWith360 = GetComponentInParent<PlayerStats>().unlockedSkills.Contains("JumpWith360");
        allowFrontflipattack = GetComponentInParent<PlayerStats>().unlockedSkills.Contains("FrontflipAttack");
        allowBackflip = GetComponentInParent<PlayerStats>().unlockedSkills.Contains("Backflip");
        //if (attacking && m.onGround) 

        if (!attacking)
        {
            if (comboTimeCounter > -1) comboTimeCounter -= Time.deltaTime;
            m.animator.SetFloat("combo", comboTimeCounter);
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
        if(!attacking && Time.timeScale != 0) m.animator.SetBool("block", block.ReadValue<float>() > 0);
        //360 attack
        m.animator.SetBool("360", do360.ReadValue<Vector2>().x > 0 && do360.ReadValue<Vector2>().y > 0 && allow360 && skillsCDTimer <= 0);
        if (m.onGround) jumpedWith360 = false;
        //Frontflip attack
        bool ffaPressed = frontflipAttack.ReadValue<Vector2>().x > 0 && frontflipAttack.ReadValue<Vector2>().y > 0 && allowFrontflipattack && skillsCDTimer <= 0;
        m.animator.SetBool("frontflipAttack", ffaPressed);
        if (ffaPressed) ShowTrajectory();
        else line.enabled = false;
        Aim(ffaPressed);

        //Backflip
        m.animator.SetBool("backflip", backflip.ReadValue<Vector2>().x > 0 && backflip.ReadValue<Vector2>().y > 0 && allowBackflip && skillsCDTimer <= 0);

        if (m.animator.GetCurrentAnimatorClipInfo(0).Length > 0)
        {
            if (m.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Jump Attack")
            {
                attackPower = jaPower;
                // skillUsed = true;
                if (m.onGround) AttackEnd(); //increases attack power and ends attack through code because animation doesn't have AttackEnd event because it has to wait for player to land
            }
            else if (m.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Falling" ||
            m.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "LandingHard" ||
            m.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "GetUp" ||
            m.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Ledge climb" ||
            m.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "InAir")
            {
                if (!attackEnded) { AttackEnd(); attackEnded = true; }
            }
            else if (m.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "FrontflipAttack")
            {
                attackPower = ffaPower;
                skillUsed = true;
                if (m.onGround) AttackEnd();
            }
            else if (m.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Actual360"|| m.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Backflip") skillUsed = true;
            else if (m.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Attack1" ||
            m.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Attack2") attackPower = defaultPower;
        }

        if(!attacking) m.animator.gameObject.transform.localEulerAngles = Vector2.zero;

        //skills cooldown
        if(skillsCDTimer > -1) skillsCDTimer -= Time.deltaTime;
        if (skillsCD != null)
        {
            if (skillsCDTimer > 0) skillsCD.gameObject.SetActive(true);
            else skillsCD.gameObject.SetActive(false);
            skillsCD.value = skillsCooldown - skillsCDTimer;
        }

        //if (m.animator.GetCurrentAnimatorClipInfo(0).Length > 0 && m.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "FrontflipAttack") m.speed = m.defaultSpeed;
    }

    // public void Roll(float x)
    // {
    //     if(allowRoll && skillsCDTimer <= 0 && x != 0 && skill.ReadValue<float>() > 0 && m.onGround)
    //     {
    //         m.animator.SetTrigger("roll");
    //         //m.animator.SetBool("isCrouching", false);
    //         if(x > 0) { x = 1; m.animator.SetBool("rollDir", true); }
    //         else { x = -1; m.animator.SetBool("rollDir", false); }
    //         m.rb.AddForce(playerTransform.right * x * attackMoveForce, ForceMode.Impulse);
    //         skillsCDTimer = 1f;
    //     }
    // }

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Enemy") && attacking)
        {
            Enemy enemy = col.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                if (col.GetComponentInChildren<Animator>().GetCurrentAnimatorClipInfo(0).Length > 0 &&                                 //check for blocking
                col.GetComponentInChildren<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name == "Block" &&
                Physics.Raycast(col.transform.position, col.transform.forward, out RaycastHit hit, 3f, 1 << 3) &&                     //check for facing to player
                hit.collider.transform.parent.GetComponentInChildren<Attack2>() == this)                                                //check is the enemy the ray hits is this player (do i need this?)
                {
                    Debug.Log("Enemy block hit!");                                                                                          //if the enemy is blocking, do nothing
                    return;
                }
                if (enemy.health > 0 && !enemy.timeStopped)
                {
                    //enemy.attacked = true;
                    Vector3 knockbackDir = playerTransform.forward * knockback;
                    knockbackDir.y = Mathf.Abs(knockbackY);
                    enemy.gameObject.GetComponent<NavMeshAgent>().enabled = false;
                    enemy.rb.AddForce(knockbackDir, ForceMode.Impulse);
                    //StartCoroutine(EnableNavMesh(enemy.gameObject /*, enemy.rb*/ ));
                    enemy.health -= attackPower;
                    print("Enemy's health left:" + enemy.health);
                    if(m.animator.GetCurrentAnimatorClipInfo(0).Length > 0 && m.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Backflip")
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
        // Vector3 startVel = playerTransform.forward * m.defaultSpeed + Vector3.up * (m.jumpForce / m.rb.mass / 1000);

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
        }
        else
        {
            mainCam.Priority = 10;
            aimCam.Priority = 0;
        }
    }
}
