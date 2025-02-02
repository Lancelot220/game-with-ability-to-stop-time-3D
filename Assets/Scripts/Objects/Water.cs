using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Water : MonoBehaviour
{
    public Transform[] pointsOnGround;
    public int damage = 8;
    private float waitTime = 3;
    [Header("Condition")]
    public bool useCondition;
    public Transform[] pointsToReach;
    public bool condition;
    Transform player;
    GameObject playerObj;
    public GameObject[] breakablesToReset;



    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            StartCoroutine(TpPlayerOnGround());
            player = col.transform;
            playerObj = col.gameObject;
            playerObj.GetComponent<Movement>().enabled = false;
            playerObj.GetComponentInChildren<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            playerObj.GetComponent<PlayerStats>().health -= damage;
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
                float distance = Mathf.Infinity;
                foreach (Transform point in pointsToReach)
                {
                    if(Vector3.Distance(player.position, point.position) < distance)
                    destination = point;
                    distance = Vector3.Distance(player.position, point.position);
                }
            }
            else
            {
                float distance = Mathf.Infinity;
                foreach (Transform point in pointsOnGround)
                {
                    if(Vector3.Distance(player.position, point.position) < distance)
                    destination = point;
                    distance = Vector3.Distance(player.position, point.position);
                }
            }
        }

        player.position = destination.position;
        playerObj.GetComponent<Movement>().enabled = true;
        playerObj.GetComponentInChildren<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;

        if(breakablesToReset.Length > 0)
        {
            foreach (GameObject breakable in breakablesToReset)
            {
                breakable.SetActive(true);
            }
        }
    }    
}
