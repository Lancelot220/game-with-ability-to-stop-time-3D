using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointOnGround : MonoBehaviour
{
    public Water water;

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            if (water != null)
            water.condition = true;
            else 
            {
                if(transform.parent.TryGetComponent<Water>(out Water water1))
                water1.condition = true;
                else Debug.LogError("HEY, WHERE'S THE WATER COMPONENT?! ADD IT. IMMEDIATELY");
            }
        }
    }
}
