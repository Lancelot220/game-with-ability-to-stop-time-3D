using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public bool condition;
    public bool closeWhenPassed;
    public bool canBeOpenedOnce;
    public bool auto = true;
    public float speed;
    //public float closedPos;
    public float opendedPos;
    bool playerHaveOpened;
    float closedPos;
    bool playerHavePassed;
    [SerializeField]bool cantOpen;
    //for opening with Interact button
    [SerializeField]bool playerInTrigger;
    [SerializeField]GameObject player;

    void Start()
    {
        closedPos = transform.localPosition.y;
    }

    void Update()
    {
        if(condition && playerHaveOpened && Mathf.Abs(transform.localPosition.y - opendedPos) > 0.05)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - (speed * Time.deltaTime), transform.localPosition.z);
        }

        if(closeWhenPassed && condition && playerHavePassed && Mathf.Abs(transform.localPosition.y - closedPos) > 0.05)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + (speed * Time.deltaTime), transform.localPosition.z);
        }

        //for opening with Interact button
        if(playerInTrigger && player != null && player.GetComponent<PlayerStats>().isInteracting && condition && !cantOpen)
        {
            playerHaveOpened = true;
            player.GetComponent<PlayerStats>().isInteracting = false;
            //print("Interacted");
            if(canBeOpenedOnce) cantOpen = true;
        }
    }

    void OnTriggerStay(Collider col)
    {
        if(col.CompareTag("Player") && condition && !cantOpen)
        {
            if(auto) playerHaveOpened = true;
            if(canBeOpenedOnce && auto) cantOpen = true;
            playerHavePassed = false;

            //for opening with Interact button
            playerInTrigger = true;
            player = col.gameObject;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if(col.CompareTag("Player"))
        {
            if(closeWhenPassed) playerHavePassed = true;
            playerHaveOpened = false;

            //for opening with Interact button
            playerInTrigger = false;
        }
    }

    public void Unlock()
    {
        condition = true;
    }
}
