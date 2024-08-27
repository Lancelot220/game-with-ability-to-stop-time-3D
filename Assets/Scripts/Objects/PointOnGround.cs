using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointOnGround : MonoBehaviour
{
    public Water Water;

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            Water.condition = true;
        }
    }
}
