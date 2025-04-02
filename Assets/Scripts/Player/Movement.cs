
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    //options
    [Header("Moving")]
    public float defaultSpeed = 10f;
    public float speed;
    public float runSpeed = 15f;
    public float maxSpeed = 5f;
    public float slideSpeed = 5f;
    public float maxAirSpeed = 10f;
    public float animSpeedMidAir = 0.1f;
    public float acceleration = 12f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float inAirRotationSpeed = 0.1f;
    private float defaultRotationSpeed;
    bool isWalking;
    
    [Header("Jumping")]
    public float jumpForce = 300f;
    public bool onGround;
    public float coyoteTime = 0.2f;
    float coyoteTimeCounter;
    public float jumpDelay = 0.5f;
    float jumpDelayCounter = 0;
    //[HideInInspector] public bool hasJumped = false;

    [Header("Ledge Climb")]
    public Transform emptySpaceRayOrigin;
    public float emptySpaceCheckLength = 0.2f;
    public Transform edgeCheckRayOrigin;
    public float edgeCheckLength = 0.1f;
    public LayerMask ledgeClimbLayers;
    public Transform spineBone;
    public bool endClimbing;
    public Vector3 spineDefaultOffset;
    bool isClimbing;
    
    [Header("Crouching")]
    public float crouchSpeed = 5f;
    MeshCollider meshCollider;
    /*[HideInInspector]*/ public bool canStopCrouching = true;
    /*[HideInInspector]*/ public bool holdingCrouchButton;
    public Mesh normalCollider;
    public Mesh crouchCollider;

    //references
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Animator animator;
    PlayerStats ps;

    Transform cameraMainTransform;
    
    //Input actions
    Controls ctrls;
    InputAction move;
    InputAction jump;
    InputAction cameraMoving;
    InputAction runStart;
    InputAction runStop;
    InputAction crouchStart;
    InputAction crouchStop;

    [Header("")]
    public bool debug = true;
    /*
    [Header("Instead of anims")]
    public Transform capsule;
    public Transform cube;
    */
    void Start()
    {
        speed = defaultSpeed;
        rb = GetComponent<Rigidbody>();
        meshCollider = GetComponent<MeshCollider>();
        cameraMainTransform = Camera.main.transform;
        animator = GetComponentInChildren<Animator>();
        ps = GetComponent<PlayerStats>();
        canStopCrouching = true;

        spineDefaultOffset = transform.position - spineBone.position;
        defaultRotationSpeed = rotationSpeed;
    }
    void Awake() {ctrls = new Controls();}
    void OnEnable()
    {
        move = ctrls.Player.Move;
        move.Enable();

        cameraMoving = ctrls.Player.Look;
        cameraMoving.Enable();

        jump = ctrls.Player.Jump;
        jump.Enable();
        jump.performed += Jump;

        runStart = ctrls.Player.RunStart;
        runStart.Enable();
        runStart.performed += RunStart;

        runStop = ctrls.Player.RunStop;
        runStop.Enable();
        runStop.performed += RunStop;

        crouchStart = ctrls.Player.CrouchStart;
        crouchStart.Enable();
        crouchStart.performed += CrouchStart;

        crouchStop = ctrls.Player.CrouchStop;
        crouchStop.Enable();
        crouchStop.performed += CrouchStop;
    }
   void OnDisable() 
    {
        move.Disable(); 
        cameraMoving.Disable(); 
        jump.Disable();
        runStart.Disable();
        runStop.Disable();
        crouchStart.Disable();
        crouchStop.Disable();
    }

   //Jump
   private void Jump(InputAction.CallbackContext context)
    {
        if( coyoteTimeCounter > 0 &&
            speed != crouchSpeed && animator.GetFloat("combo") <= 0 && 
            jumpDelayCounter <= 0 &&
            !ps.pauseMenu.activeSelf )
        {
            rb.AddForce(new Vector3(rb.velocity.x, jumpForce, rb.velocity.z));
            coyoteTimeCounter = 0;
            animator.SetBool("jumped", true);
           
            jumpDelayCounter = jumpDelay;
            //hasJumped = true;
            //print("hop");
        }
    }
    
    void Update() 
    {
        animator.SetBool("onGround", onGround);

        if (!animator.GetBool("isRunning") && !animator.GetBool("isCrouching") && animator.GetFloat("combo") <= 0 /*&& onGround*/)
        speed = defaultSpeed;

        jumpDelayCounter -= Time.deltaTime;        

        //animator destroyed fix
        //if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    //Run
    private void RunStart(InputAction.CallbackContext context)
    {
        if( speed != crouchSpeed && 
            animator.GetFloat("combo") <= 0 && 
            onGround &&
            !ps.pauseMenu.activeSelf )
        {
            //speed = runSpeed;
            isWalking = true;
            animator.SetBool("isRunning", true);
        }
    }
    private void RunStop(InputAction.CallbackContext context)
    { 
        if( speed == runSpeed && 
            animator.GetFloat("combo") <= 0 &&
            !ps.pauseMenu.activeSelf )
        {
            //speed = defaultSpeed;
            isWalking = false;
            animator.SetBool("isRunning", false);
        }
    }
    
    //Crouch
    private void CrouchStart(InputAction.CallbackContext context)
    {
        holdingCrouchButton = true;
        if( /*speed !=runSpeed &&*/onGround &&
            animator.GetFloat("combo") <= 0 &&
            !ps.pauseMenu.activeSelf )
        {
            speed = crouchSpeed;
            //cc.height = 1;
            //cc.center = new Vector3(0, -0.5f, 0);
            meshCollider.sharedMesh = crouchCollider;

            animator.SetBool("isCrouching", true);
            /*
            //instead of anim
            capsule.localPosition = new Vector3(0, -0.5f, 0);
            capsule.localScale = new Vector3(1, 0.5f, 1);

            cube.localPosition = new Vector3(cube.localPosition.x, -0.5f, cube.localPosition.z);
            */
        }
    }
    private void CrouchStop(InputAction.CallbackContext context)
    {
        holdingCrouchButton = false;
        if( speed == crouchSpeed &&
            canStopCrouching &&
            !ps.pauseMenu.activeSelf ) 
        {
            CrouchStop_();
        } 
    }
    public void CrouchStop_()
    {
        
        //cc.height = 2;
        //cc.center = Vector3.zero;
        meshCollider.sharedMesh = normalCollider;

        animator.SetBool("isCrouching", false);

        if (!animator.GetBool("isRunning") && !animator.GetBool("isCrouching"))
        speed = defaultSpeed;
        /*
        //instead of anim
        capsule.localPosition = Vector3.zero;
        capsule.localScale = Vector3.one;

        cube.localPosition = new Vector3(cube.localPosition.x, 0.3821f, cube.localPosition.z);
        */
    }
    
    //Move
    void FixedUpdate()
    {
        Vector2 dir = move.ReadValue<Vector2>();
        if(isWalking) dir *= 0.5f;
        Vector3 movement = new Vector3(dir.x, 0f, dir.y) * speed;
        //movement = cameraMainTransform.forward * movement.z + cameraMainTransform.right * movement.x;
        
        Vector3 cameraForward = cameraMainTransform.forward;
        cameraForward.y = 0;  // Ігноруємо нахил камери по осі Y
        cameraForward.Normalize();  // Нормалізуємо вектор, щоб зберегти його напрямок
        Vector3 cameraRight = cameraMainTransform.right;
        cameraRight.y = 0;  // Також ігноруємо можливі зміщення по осі Y у правого вектора
        cameraRight.Normalize();
        movement = cameraForward * movement.z + cameraRight * movement.x;
        
        movement.y = 0f;

        
        if (animator.GetFloat("combo") <= 0 && 
        onGround && animator.GetCurrentAnimatorClipInfo(0).Length > 0 &&
        animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "LandingHard" && 
        animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "GetUp") 
        {
            rotationSpeed = defaultRotationSpeed;
            // Використовуємо Lerp для плавного руху
            rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(movement.x, rb.velocity.y, movement.z), Time.deltaTime * acceleration);
            //rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);
        }
        else if (!onGround && animator.GetFloat("combo") <= 0 && dir != Vector2.zero && !isClimbing) //air physics
        { 
            rotationSpeed = inAirRotationSpeed;
            //rb.AddForce(movement.normalized * inAirSpeed, ForceMode.Acceleration);

            // Зберігаємо горизонтальну швидкість, але з контролем
            Vector3 airMovement = new Vector3(movement.x, rb.velocity.y, movement.z);

            // Плавне наближення швидкості в повітрі до бажаного напрямку
            airMovement.x = Mathf.Lerp(rb.velocity.x, movement.x, Time.deltaTime * acceleration * 0.1f);
            airMovement.z = Mathf.Lerp(rb.velocity.z, movement.z, Time.deltaTime * acceleration * 0.1f);

            // Обмежуємо максимальну швидкість (для уникнення "нескінченного прискорення")
            airMovement.x = Mathf.Clamp(airMovement.x, -maxAirSpeed, maxAirSpeed);
            airMovement.z = Mathf.Clamp(airMovement.z, -maxAirSpeed, maxAirSpeed);

            // Застосовуємо змінене значення швидкості
            rb.velocity = airMovement;
        }

        if (dir != Vector2.zero && 
        !isClimbing &&
        animator.GetCurrentAnimatorClipInfo(0).Length > 0 &&
        animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "LandingHard" && 
        animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "GetUp")
        {
            if (onGround) animator.SetBool("isMoving", true);
        
            float targetAngle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg + cameraMainTransform.eulerAngles.y;
            Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation , rotation, Time.deltaTime * rotationSpeed);
        }
        else  animator.SetBool("isMoving", false);
        
        /*if (move.ReadValue<Vector2>() != Vector2.zero)*/ animator.SetFloat("speed", Vector2.Distance(Vector2.zero, dir));
        //if(!onGround) animator.SetFloat("speed", animSpeedMidAir);

        //Coyote time
        if(onGround)
        { coyoteTimeCounter = coyoteTime; } else { coyoteTimeCounter -= Time.deltaTime; } 

        //limit vertical speed
        Vector3 velocity = rb.velocity;
        if (velocity.y > 0)
        {
            velocity.y = Mathf.Min(velocity.y, maxSpeed);
        }
        rb.velocity = velocity;

        //Ledge climb
        bool emptySpaceAboveEdge = !Physics.Raycast(emptySpaceRayOrigin.position, transform.forward, emptySpaceCheckLength, ledgeClimbLayers);
        if(debug) Debug.DrawLine(emptySpaceRayOrigin.position, emptySpaceRayOrigin.position + (transform.forward * emptySpaceCheckLength), Color.red);
        bool edge = Physics.Raycast(edgeCheckRayOrigin.position, transform.forward, edgeCheckLength, ledgeClimbLayers);
        if(debug) Debug.DrawLine(edgeCheckRayOrigin.position, edgeCheckRayOrigin.position + (transform.forward * edgeCheckLength), Color.red);
        if(emptySpaceAboveEdge && edge && !onGround)
        {
            animator.SetTrigger("ledgeClimb");
            rb.constraints = RigidbodyConstraints.FreezeAll;
            isClimbing = true;
            if(debug) print("climb");
        }
        if (endClimbing)
        {
            Vector3 destination = spineBone.position + spineDefaultOffset;
            transform.position = destination;
            
            rb.constraints = RigidbodyConstraints.None;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            endClimbing = false;
            isClimbing = false;
            animator.ResetTrigger("ledgeClimb");
        }

        if (rb.constraints == RigidbodyConstraints.FreezeAll)
        {
            if(animator.GetCurrentAnimatorClipInfo(0).Length > 0)  { if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Ledge climb") { endClimbing = true; } }
            else { endClimbing = true; }
        }
    }  //.clip.name != "Attack1" &&  != "Attack2" && animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Crit"
    /*public void EndClimbing()
    {
        
    }*/

    void OnTriggerStay(Collider col)
    {
        if(col.CompareTag("SlideDown"))
        rb.velocity = new Vector3(rb.velocity.x, -slideSpeed, rb.velocity.z);
    }
}
