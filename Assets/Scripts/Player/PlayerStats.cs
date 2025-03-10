
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor.Callbacks;
//using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    [Header("Statistics")]
    public int health = 100;
    /*
    [SerializeField] int defaultLivesCount = 5;
    public int Lives //= 5;
    {
        get { return PlayerPrefs.GetInt("lives", defaultLivesCount); }
        set { PlayerPrefs.SetInt("lives", value); } 
    }*/
    public int orbsCollected;
    [ReadOnly] public float time;

    [Header("Fall Damage")]
    public float fallVelocity;
    public float fVThreshold = -10;
    public float delayBeforeDeathScreen = 0.75f;
    public float fallDamageMultipier = 1;

    [Header("Others")]
    public float minWorldHeightLimit = -100;
    public bool isHiding;
    
    Animator screenAnim;
    bool hitGroundTooHard;
    bool deathMessageSent = false;
    int previousOrbsCollected = 0;
    int fallDamage;
    Movement m;
    [Header("Pause & Interaction")]
    public GameObject pauseMenu;
    InputAction pause;
    InputAction interact;
    Controls ctrls;
    [HideInInspector] public bool isInteracting;
    
    void Start() 
    {
        m = GetComponentInParent<Movement>();
        screenAnim = GameObject.Find("HUD").GetComponent<Animator>();
        //lives = PlayerPrefs.GetInt("lives", lives);
        ctrls = new Controls();
    }

    void Awake() { ctrls = new Controls(); }
    void OnEnable()
    {
        pause = ctrls.Player.Pause;
        pause.Enable();
        pause.performed += Pause;

        interact = ctrls.Player.Interact;
        interact.Enable();
        interact.performed += Interact;
    }
    void OnDisable()
    {
        pause.Disable();
        interact.Disable();
    }

    void Update()
    {   //fall damage
        if(!m.onGround) fallVelocity = m.rb.velocity.y;
        if(fallVelocity < fVThreshold)
        {
            m.animator.SetBool("isFalling", true);
            fallDamage = Convert.ToInt32(fallVelocity * fallVelocity * fallDamageMultipier);
        }

        //Death
        if ((health <= 0 || transform.position.y < minWorldHeightLimit) && !deathMessageSent)
        {
            health = 0;
            Debug.LogError("СМЕРТЬ");
            deathMessageSent = true;
             
            m.speed = 0; m.defaultSpeed = 0;
            m.jumpForce = 0; m.runSpeed = 0; m.crouchSpeed = 0;
            m.enabled = false;
            if (!hitGroundTooHard) m.animator.SetBool("died", true);

            StartCoroutine(Rumble.RumblePulse(0.25f, 1f, 3f));

            StartCoroutine(Restart());
            screenAnim.SetTrigger("LevelLoad");
        }

        //time
        time += Time.deltaTime;
        
        /*lives
        if(orbsCollected % 50 == 0 && orbsCollected != 0 && orbsCollected != previousOrbsCollected)
        { 
            Lives++; //PlayerPrefs.SetInt("lives", lives); 
            previousOrbsCollected = orbsCollected;
        }*/
    }

    public void Landed()
    {
        if(m.animator.GetBool("isFalling"))
        {
            m.animator.SetTrigger("landedHard");
            m.animator.SetBool("isFalling", false);
            m.rb.velocity = Vector3.zero;
            health -= fallDamage;
            Debug.LogWarning($"You've lost {fallDamage} hp by falling!");
            fallDamage = 0;
            fallVelocity = 0;
            if(health > 0) 
                m.animator.SetTrigger("survived");
            else 
                hitGroundTooHard = true;

            StartCoroutine(Rumble.RumblePulse(0.25f, 1f, 1f));
        }
        else 
        {
            m.animator.SetTrigger("landedSucces");
            StartCoroutine(Rumble.RumblePulse(0.25f, 0.25f, 0.25f));
        }
    }

    IEnumerator Restart()
    {
        yield return new WaitForSecondsRealtime(delayBeforeDeathScreen);

        //Scene scene = SceneManager.GetActiveScene();
        //PlayerPrefs.SetInt("lastLevel",SceneManager.GetActiveScene().buildIndex);
        SceneManager.LoadScene("Death Screen");
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

    void Interact(InputAction.CallbackContext context)
    {
        isInteracting = true;
        StartCoroutine(DisableInteraction());
        print("trying to interact");
    }

    IEnumerator DisableInteraction() {yield return new WaitForSeconds(0.5f); isInteracting = false;}
}
