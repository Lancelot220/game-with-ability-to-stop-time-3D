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
        if(!col.CompareTag("Player") && !col.CompareTag("Trigger"))
        {
            movement.onGround = true; //print("bruh");
            ps.Landed();
            movement.animator.SetBool("jumped", false);
            //movement.hasJumped = false;
            GetComponent<AudioSource>().Play();
        }

        if (col.CompareTag("Metal")) footsteps.currsentSurface = Footsteps.Surface.Metal;
        else if (col.CompareTag("Grass")) footsteps.currsentSurface = Footsteps.Surface.Grass;
        else footsteps.currsentSurface = Footsteps.Surface.Metal;
    }
    void OnTriggerStay(Collider col)
    {
        if(!col.CompareTag("Player") && !col.CompareTag("Trigger"))
        {
            movement.onGround = true; //print("bruh");
            //if(movement.animator.GetBool("jumped")) 
            movement.animator.SetBool("jumped", false); 
        }

        if (col.CompareTag("Metal")) footsteps.currsentSurface = Footsteps.Surface.Metal;
        else if (col.CompareTag("Grass")) footsteps.currsentSurface = Footsteps.Surface.Grass;
        else footsteps.currsentSurface = Footsteps.Surface.Metal;
    }
    
    void OnTriggerExit(Collider col)
    {
        if(!col.CompareTag("Player") && !col.CompareTag("Trigger"))
        { movement.onGround = false; /*Debug.LogWarning("huh?");*/ }
    }
}