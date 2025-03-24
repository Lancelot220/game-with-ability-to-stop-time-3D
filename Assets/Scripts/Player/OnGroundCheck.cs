using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class OnGroundCheck : MonoBehaviour
{
    Movement movement;
    PlayerStats ps;
    Footsteps footsteps;

    void Start() 
    {
        movement = GetComponentInParent<Movement>(); 
        ps = GetComponentInParent<PlayerStats>(); 
        footsteps = transform.parent.GetComponentInChildren<Footsteps>();
    } 
    void OnTriggerEnter(Collider col)
    {
        if(!col.CompareTag("Player") && !col.CompareTag("Trigger") && !col.CompareTag("Enemy"))
        {
            movement.onGround = true;
            ps.Landed();
            movement.animator.SetBool("jumped", false);
            //movement.hasJumped = false;
            footsteps.PlaySound();
        }
        if (col.CompareTag("Enemy"))
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if(enemy.timeStopped)
            {
                movement.onGround = true;
                ps.Landed();
                movement.animator.SetBool("jumped", false);
                footsteps.PlaySound();
            }
        }

        if (col.CompareTag("Metal")) footsteps.currsentSurface = Footsteps.Surface.Metal;
        else if (col.CompareTag("Grass")) footsteps.currsentSurface = Footsteps.Surface.Grass;
        else footsteps.currsentSurface = Footsteps.Surface.Metal;
    }
    void OnTriggerStay(Collider col)
    {
        if(!col.CompareTag("Player") && !col.CompareTag("Trigger") && !col.CompareTag("Enemy"))
        {
            movement.onGround = true;
            movement.animator.SetBool("jumped", false); 
        }
        if (col.CompareTag("Enemy"))
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if(enemy.timeStopped)
            {
                movement.onGround = true;
                movement.animator.SetBool("jumped", false);
            }
        }

        if (col.CompareTag("Metal")) footsteps.currsentSurface = Footsteps.Surface.Metal;
        else if (col.CompareTag("Grass")) footsteps.currsentSurface = Footsteps.Surface.Grass;
        else footsteps.currsentSurface = Footsteps.Surface.Metal;
    }
    
    void OnTriggerExit(Collider col)
    {
        if(!col.CompareTag("Player") && !col.CompareTag("Trigger") && !col.CompareTag("Enemy"))
        { movement.onGround = false; /*Debug.LogWarning("huh?"); movement.animator.SetBool("jumped", true);*/ }
        if (col.CompareTag("Enemy"))
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if(enemy.timeStopped)
            {
                movement.onGround = false;
            }
        }
    }
}