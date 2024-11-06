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
    
    //public float inAirSpeed = 1.1f;
    [SerializeField] private float rotationSpeed = 3f;
    
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
    [Header("Pause")]
    public GameObject pauseMenu;

    //references
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Animator animator;

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
    InputAction pause;

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
        canStopCrouching = true;

        spineDefaultOffset = transform.position - spineBone.position;
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

        pause = ctrls.Player.Pause;
        pause.Enable();
        pause.performed += Pause;
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
        pause.Disable();
    }

   //Jump
   private void Jump(InputAction.CallbackContext context)
    {
        if( coyoteTimeCounter > 0 &&
            speed != crouchSpeed && animator.GetFloat("combo") <= 0 && 
            jumpDelayCounter <= 0 &&
            !pauseMenu.activeSelf )
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
            !pauseMenu.activeSelf )
        {
            speed = runSpeed;
            animator.SetBool("isRunning", true);
        }
    }
    private void RunStop(InputAction.CallbackContext context)
    { 
        if( speed == runSpeed && 
            animator.GetFloat("combo") <= 0 &&
            !pauseMenu.activeSelf )
        {
            speed = defaultSpeed;
            animator.SetBool("isRunning", false);
            //
        }
    }
    
    //Crouch
    private void CrouchStart(InputAction.CallbackContext context)
    {
        holdingCrouchButton = true;
        if( speed !=runSpeed &&
            animator.GetFloat("combo") <= 0 &&
            !pauseMenu.activeSelf )
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
            !pauseMenu.activeSelf ) 
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
        Vector2 dir = move.ReadValue<Vector2>() * speed;
        Vector3 movement = new Vector3(dir.x, 0f, dir.y);
        //movement = cameraMainTransform.forward * movement.z + cameraMainTransform.right * movement.x;
        
        Vector3 cameraForward = cameraMainTransform.forward;
        cameraForward.y = 0;  // Ігноруємо нахил камери по осі Y
        cameraForward.Normalize();  // Нормалізуємо вектор, щоб зберегти його напрямок
        Vector3 cameraRight = cameraMainTransform.right;
        cameraRight.y = 0;  // Також ігноруємо можливі зміщення по осі Y у правого вектора
        cameraRight.Normalize();
        movement = cameraForward * movement.z + cameraRight * movement.x;
        
        movement.y = 0f;
        if (animator.GetFloat("combo") <= 0 && onGround) 
        { rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z); }
        else if (!onGround && animator.GetFloat("combo") <= 0 && dir != Vector2.zero)
        { rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z); }

        if (dir != Vector2.zero && !isClimbing)
        {
            if (onGround) animator.SetBool("isMoving", true);
        
            float targetAngle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg + cameraMainTransform.eulerAngles.y;
            Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation , rotation, Time.deltaTime * rotationSpeed);
        }
        else  animator.SetBool("isMoving", false);
        
        if (move.ReadValue<Vector2>() != Vector2.zero) animator.SetFloat("speed", Vector2.Distance(Vector2.zero, move.ReadValue<Vector2>()));

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
    }  //animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Attack1" && animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Attack2" && animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Crit"
    /*public void EndClimbing()
    {
        
    }*/

    void OnTriggerStay(Collider col)
    {
        if(col.CompareTag("SlideDown"))
        rb.velocity = new Vector3(rb.velocity.x, -slideSpeed, rb.velocity.z);
    }
    
    void Pause(InputAction.CallbackContext context)
    {
        if(!pauseMenu.activeSelf)
        {
            Time.timeScale = 0;
            pauseMenu.SetActive(true);
        }
        else
        {
            Unpause();
        }
    }
    public void Unpause()
    {
        if (pauseMenu.activeSelf)
        {
            Time.timeScale = 1;
            pauseMenu.SetActive(false);

            foreach (GameObject popup in GameObject.FindGameObjectsWithTag("Popup")) { popup.SetActive(false); }
        }
    }
}
