using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class OnGroundCheck : MonoBehaviour
{
    Movement movement;
    PlayerStats ps;
    void Start() {movement = GetComponentInParent<Movement>(); ps = GetComponentInParent<PlayerStats>();} 
    void OnTriggerEnter(Collider col)
    {
        if(!col.CompareTag("Player") && !col.CompareTag("Trigger"))
        {
            movement.onGround = true; print("bruh");
            ps.Landed();
            movement.animator.SetBool("jumped", false);
        }
    }

    void OnTriggerStay(Collider col)
    {
        if(!col.CompareTag("Player") && !col.CompareTag("Trigger"))
        {
            movement.onGround = true; //print("bruh");
            //if(movement.animator.GetBool("jumped")) 
            movement.animator.SetBool("jumped", false); 
        }
    }

    void OnTriggerExit(Collider col)
    {
        if(!col.CompareTag("Player") && !col.CompareTag("Trigger"))
        { movement.onGround = false; Debug.LogWarning("huh?"); }
    }
}