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
        movement.onGround = true;
        ps.Landed();
        if(movement.animator.GetBool("jumped")) 
        { movement.animator.SetBool("jumped", false); }
    }
    void OnTriggerStay(Collider col)
    {
        movement.onGround = true;
        if(movement.animator.GetBool("jumped")) 
        { movement.animator.SetBool("jumped", false); }
    }
    void OnTriggerExit(Collider col)
    {
        movement.onGround = false; 
    }
}