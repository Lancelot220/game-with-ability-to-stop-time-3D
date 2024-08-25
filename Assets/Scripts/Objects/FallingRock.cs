using System;
using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;

public class FallingRock : MonoBehaviour
{ 
    public float minWorldHeightLimit = -100;
    public bool lastRock;
    public Water water;
    Vector3 startPos;
    Rigidbody rb;

    void Start() {startPos = transform.position; rb = GetComponent<Rigidbody>(); }
    void Update()
    {
        if (transform.position.y < minWorldHeightLimit)
        {
            transform.position = startPos;
            rb.velocity = Vector3.zero;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if(col.collider.CompareTag("Player") && lastRock)
        { water.condition = true; }
    }
}
