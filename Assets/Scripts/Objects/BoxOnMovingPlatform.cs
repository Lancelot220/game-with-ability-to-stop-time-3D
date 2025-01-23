using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxOnMovingPlatform : MonoBehaviour
{
    void OnCollisionEnter(Collision col)
    {
        if (col.collider.CompareTag("Player"))
        {
            col.transform.SetParent(transform);
        }
    }

    void OnCollisionExit(Collision col)
    {
        if (col.collider.CompareTag("Player"))
        {
            col.transform.SetParent(null);
        }
    }
}
