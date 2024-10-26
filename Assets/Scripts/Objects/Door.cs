using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public bool condition;
    public float speed;
    //public float closedPos;
    public float opendedPos;
    bool playerHaveOpened;

    void Update()
    {
        if(condition && playerHaveOpened && transform.position.y >= opendedPos)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - (speed * Time.deltaTime), transform.position.z);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.CompareTag("Player") && condition)
        {
            playerHaveOpened = true;
        }
    }
}
