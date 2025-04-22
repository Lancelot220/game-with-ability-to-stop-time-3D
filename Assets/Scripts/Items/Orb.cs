using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour
{
    public float atractionRange = 3f;
    public float atractionForce = 15f;
    Rigidbody rb;
    Transform player;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    
    void FixedUpdate()
    {
        if(player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        if(distance < atractionRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            rb.AddForce(direction * atractionForce);
        }
    }
    
    public void ApplyEffect(GameObject player)
    {
        PlayerStats ps = player.GetComponent<PlayerStats>();
        ps.orbsCollected++;
    }
}
