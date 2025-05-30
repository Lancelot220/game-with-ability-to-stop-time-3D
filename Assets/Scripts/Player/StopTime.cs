using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    
    
    public LayerMask stoppableObjects;
    public bool canStopTime = true;
    
    //stop time mechanic
    [HideInInspector] public float durationTimer;
    [HideInInspector] public float cdTimer;
    Collider[] objectsInRange;
    Vector3 navMeshAgentDst;
    [HideInInspector] public bool timeStopped;

    //references
    Controls ctrls;
    InputAction stopTime;
    Slider slider;

    //efects
    public AudioSource stopTimeSound;
    public AudioSource unfreezeTimeSound;
    //public ParticleSystem effect;
    public GameObject effectGO;
    GameObject effectOnScene;

    void Awake() { ctrls = new Controls(); slider = GameObject.Find("Stop Time").GetComponent<Slider>();}
    void OnEnable() { stopTime = ctrls.Player.StopTime; stopTime.Enable(); stopTime.performed += StopTime; }
    void OnDisable() { stopTime.Disable(); }
    void Start() { effectGO.transform.SetParent(null); } 

    void Update()
    {
        if(durationTimer > 0 && !canStopTime && slider != null)
        { slider.maxValue = duration; slider.value = durationTimer;}
        if(cdTimer < cd && !canStopTime && durationTimer <= 0 && slider != null)
        { slider.maxValue = cd; slider.value = cdTimer;}
        
        //unfreeze
        if(timeStopped && durationTimer > 0)
        { durationTimer -= Time.deltaTime; }
        else if (durationTimer <= 0 && timeStopped)
        { timeStopped = false;  UnfreezeTime(); }

        //cd
        if(!canStopTime && cdTimer < cd)
        { cdTimer += Time.deltaTime; }
        else if (!canStopTime && cdTimer >= cd)
        { canStopTime = true; cdTimer = 0; print("You can stop time again!"); unfreezeTimeSound.Play(); slider.value = cd; }
    }

    //stop time
    void StopTime(InputAction.CallbackContext context)
    {
        if(canStopTime && Time.timeScale != 0)
        {
            canStopTime = false;

            print("Time has been stopped!"); timeStopped = true;
            
            objectsInRange = Physics.OverlapSphere(transform.position, range, stoppableObjects);
            
            foreach (Collider obj in objectsInRange)
            {
                Rigidbody rb = obj.gameObject.GetComponent<Rigidbody>();
                Enemy enemy = obj.gameObject.GetComponent<Enemy>();
                EnemyWithGun enemyWithGun = obj.gameObject.GetComponent<EnemyWithGun>();
                NavMeshAgent navMeshAgent = obj.gameObject.GetComponent<NavMeshAgent>();
                StoppableObject so = obj.gameObject.GetComponent<StoppableObject>();
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

                if(so != null) so.TimeStopped();
            }

            //StartCoroutine(UnfreezeTime());

            durationTimer = duration;

            //effects
            stopTimeSound.Play();
            //effectOnScene = Instantiate(effectGO, transform.position, Quaternion.identity);
            effectGO.SetActive(true);
            effectGO.transform.position = transform.position;
            effectGO.GetComponent<Animator>().SetTrigger("stop");
            //StartCoroutine(PauseEffect());
            //effect.Play(true);
            //effectOnScene.Stop();
            //Destroy(effectOnScene, 2);
        }
    }

    IEnumerator PauseEffect()
    {
        yield return new WaitForSeconds(.5f); //0.12f
        //effectOnScene.GetComponent<ParticleSystem>().Pause(true);
        effectGO.SetActive(false);
    }
    void UnfreezeTime()
    {
        slider.value = 0;
        //yield return new WaitForSeconds(duration);

        if(objectsInRange.Length > 0)
        {
            foreach (Collider obj in objectsInRange)
            {
                Rigidbody rb = obj.gameObject.GetComponent<Rigidbody>();
                Enemy enemy = obj.gameObject.GetComponent<Enemy>();
                EnemyWithGun enemyWithGun = obj.gameObject.GetComponent<EnemyWithGun>();
                NavMeshAgent navMeshAgent = obj.gameObject.GetComponent<NavMeshAgent>();
                StoppableObject so = obj.gameObject.GetComponent<StoppableObject>();

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

                if(so != null) so.TimeUnfreezed();
            }
        }

        objectsInRange = null;
        //StartCoroutine(Cooldown());
        
        print("Time has been unfreezed.");

        cdTimer = 0;

        //effects
        unfreezeTimeSound.Play();
        effectGO.GetComponent<Animator>().SetTrigger("unfreeze");
        StartCoroutine(PauseEffect());
        //effectOnScene.GetComponent<ParticleSystem>().Play(true);
        //Destroy(effectOnScene, 1.88f);
    }

    public void ForceUnfreezeTime()
    {
        if(objectsInRange.Length > 0)
        {
            foreach (Collider obj in objectsInRange)
            {
                Rigidbody rb = obj.gameObject.GetComponent<Rigidbody>();
                Enemy enemy = obj.gameObject.GetComponent<Enemy>();
                EnemyWithGun enemyWithGun = obj.gameObject.GetComponent<EnemyWithGun>();
                NavMeshAgent navMeshAgent = obj.gameObject.GetComponent<NavMeshAgent>();
                StoppableObject so = obj.gameObject.GetComponent<StoppableObject>();

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

                if(so != null) so.TimeUnfreezed();
            }
        }

        objectsInRange = null;
        //StartCoroutine(Cooldown());
        
        print("Time has been force unfreezed.");
        canStopTime = true; 
        cdTimer = 0; 
        print("You can stop time again!"); 
        slider.value = cd;
        timeStopped = false;
        durationTimer = 0;

        //effects
        unfreezeTimeSound.Play();
        effectGO.GetComponent<Animator>().SetTrigger("unfreeze");
        StartCoroutine(PauseEffect());
        //effectOnScene.GetComponent<ParticleSystem>().Play(true);
        //Destroy(effectOnScene, 1.88f);
    }
}
