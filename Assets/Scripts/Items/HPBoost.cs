using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class HPBoost : MonoBehaviour
{
    public int hpboost = 15;
    public float rotationSpeed = 100;

    void Update()
    {
        transform.RotateAround(transform.position, Vector3.up, rotationSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.CompareTag("Player"))
        {
            PlayerStats ps = col.gameObject.GetComponent<PlayerStats>();
            ps.health += hpboost;
            if(ps.health > 100) ps.health = 100;
            print($"Now you have {ps.health} HP!");
            Destroy(gameObject);
        }
    }
}
