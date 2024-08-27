using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    public bool useCondition;
    public Transform[] pointsOnGround;
    private float waitTime = 3;
    public bool condition;
    Transform player;
    GameObject playerObj;

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            StartCoroutine(TpPlayerOnGround());
            player = col.transform;
            playerObj = col.gameObject;
            playerObj.GetComponent<Movement>().enabled = false;
            playerObj.GetComponentInChildren<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    IEnumerator TpPlayerOnGround()
    {
        yield return new WaitForSeconds(waitTime);

        
        Transform destination = null;
        if(!useCondition)
        {
            float distance = Mathf.Infinity;
            foreach (Transform point in pointsOnGround)
            {
                if(Vector3.Distance(player.position, point.position) < distance)
                destination = point;
                distance = Vector3.Distance(player.position, point.position);
            }
        }
        else
        {
            if (condition)
            {
                destination = pointsOnGround[1];
            }
            else
            {
                destination = pointsOnGround[0];
            }
        }
        //make two arrays for points (one for !condition and second for condition == true)

        player.position = destination.position;
        playerObj.GetComponent<Movement>().enabled = true;
        playerObj.GetComponentInChildren<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
    }
}
