using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class CeilingCheck : MonoBehaviour
{
    //[SerializeField] private LayerMask ceiling;
    Movement movement;
    void Start() { movement = GetComponentInParent<Movement>(); } 
    void OnTriggerEnter(Collider col)
    { if(col.gameObject.layer == 0 || col.gameObject.layer == 6) movement.canStopCrouching = false; } //!col.CompareTag("Player") && !col.CompareTag("Bullet")
    void OnTriggerStay(Collider col)
    { if(col.gameObject.layer == 0 || col.gameObject.layer == 6) movement.canStopCrouching = false; }
    void OnTriggerExit(Collider col)
    { 
        if(col.gameObject.layer == 0 || col.gameObject.layer == 6)
        {
            movement.canStopCrouching = true;
            if(!movement.holdingCrouchButton)
            { movement.CrouchStop_(); }
        }
    }
}