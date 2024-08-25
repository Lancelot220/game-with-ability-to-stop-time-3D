using System.Collections;
using System.Collections.Generic;
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
    
    //public float inAirSpeed = 1.1f;
    [SerializeField] private float rotationSpeed = 3f;
    [HideInInspector] public bool movementStopped;
    [HideInInspector] public bool atacking;
    
    [Header("Jumping")]
    public float jumpForce = 300f;
    public bool onGround;
    public float coyoteTime = 0.2f;
    float coyoteTimeCounter;
    
    [Header("Crouching")]
    public float crouchSpeed = 5f;
    MeshCollider meshCollider;
    /*[HideInInspector]*/ public bool canStopCrouching = true;
    /*[HideInInspector]*/ public bool holdingCrouchButton;
    public Mesh normalCollider;
    public Mesh crouchCollider;

    [Header("Slope Physics")]
    public float maxSlopeAngle = 45f;
    public float slideForce = 5f;
    public float maxSlideSpeed = 10f;
    public LayerMask ground;

    private RaycastHit hit;
    private float slopeAngle;

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
        if(coyoteTimeCounter > 0 && speed != crouchSpeed && animator.GetFloat("combo") <= 0)
        {
            rb.AddForce(new Vector3(rb.velocity.x, jumpForce, rb.velocity.z));
            coyoteTimeCounter = 0;
            animator.SetBool("jumped", true);
        }
    }
    
    void Update() 
    {
        animator.SetBool("onGround", onGround);

        if (!animator.GetBool("isRunning") && !animator.GetBool("isCrouching") && animator.GetFloat("combo") <= 0 /*&& onGround*/)
        speed = defaultSpeed;

        //Coyote time
        if(onGround)
        { coyoteTimeCounter = coyoteTime;} else { coyoteTimeCounter -= Time.deltaTime; } 

        //animator destroyed fix
        //if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    //Run
    private void RunStart(InputAction.CallbackContext context)
    {
        if(speed != crouchSpeed && animator.GetFloat("combo") <= 0 && onGround)
        {
            speed = runSpeed;
            animator.SetBool("isRunning", true);
        }
    }
    private void RunStop(InputAction.CallbackContext context)
    { 
        if (speed == runSpeed && animator.GetFloat("combo") <= 0)
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
        if(speed !=runSpeed && animator.GetFloat("combo") <= 0)
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
        if (speed == crouchSpeed && canStopCrouching) 
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
        movement = cameraMainTransform.forward * movement.z + cameraMainTransform.right * movement.x;
        movement.y = 0f;
        if (animator.GetFloat("combo") < 0) 
        { rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z); movementStopped = false; }
        //else if (onGround && !movementStopped)
        //{rb.velocity = Vector3.zero; print("stop player"); movementStopped = true; }

        if (dir != Vector2.zero)
        {
            animator.SetBool("isMoving", true);
        
            float targetAngle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg + cameraMainTransform.eulerAngles.y;
            Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation , rotation, Time.deltaTime * rotationSpeed);
        }
        else animator.SetBool("isMoving", false);
        
        if (move.ReadValue<Vector2>() != Vector2.zero) animator.SetFloat("speed", Vector2.Distance(Vector2.zero, move.ReadValue<Vector2>()));
        
        //slope fix
        // Check for ground and calculate slope angle
        if (Physics.Raycast(transform.position, Vector3.down, out hit, ground, 5))
        {
            slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            Debug.Log("Slope angle: " + slopeAngle); // For debugging
            
        }

        Debug.DrawLine(transform.position, transform.position + Vector3.down * 5, Color.red, 1);

        // Prevent movement on steep slopes
        if (slopeAngle > maxSlopeAngle)
        {
            // ... your existing code ...
            Debug.Log("Slope too steep, preventing movement"); // For debugging
        }

    }  //animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Attack1" && animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Attack2" && animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Crit"
}
