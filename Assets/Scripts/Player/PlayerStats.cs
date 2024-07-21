using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Callbacks;
//using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    Movement m;
    public float fallVelocity;
    public float fVThreshold = -10;
    public float delayBfRestart = 5;
    bool hitGroundTooHard;
    //Health
    public int health = 100;
    private bool deathMessageSent = false;

    public float minWorldHeightLimit = -100;
    //fall damage
    int fallDamage;
    float fallDamageMultipier = 1;
    void Start() {m = GetComponentInParent<Movement>();}
    
    void Update()
    {   //fall damage
        if(!m.onGround) fallVelocity = m.rb.velocity.y;
        if(fallVelocity < fVThreshold)
        {
            m.animator.SetBool("isFalling", true);
            fallDamage = Convert.ToInt32(fallVelocity * -fallDamageMultipier - fVThreshold);
        }

        //Death
        if ((health <= 0 || transform.position.y < minWorldHeightLimit) && !deathMessageSent)
        {
            health = 0;
            Debug.LogError("СМЕРТЬ");
            deathMessageSent = true;
             
            m.speed = 0; m.defaultSpeed = 0;
            m.jumpForce = 0; m.runSpeed = 0; m.crouchSpeed = 0;
            if (!hitGroundTooHard) m.animator.SetBool("died", true);

            StartCoroutine(Restart());
        }
    }

    public void Landed()
    {
        if(m.animator.GetBool("isFalling"))
        {
            m.animator.SetTrigger("landedHard");
            m.animator.SetBool("isFalling", false);
            health -= fallDamage;
            Debug.LogWarning($"You've lost {fallDamage} hp by falling!");
            fallDamage = 0;
            fallVelocity = 0;
            if(health > 0) 
                m.animator.SetTrigger("survived");
            else 
                hitGroundTooHard = true;
        }
        else {m.animator.SetTrigger("landedSucces");}
    }

    IEnumerator Restart()
    {
        yield return new WaitForSeconds(delayBfRestart);

        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }
}
