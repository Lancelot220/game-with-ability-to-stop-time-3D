using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bush : MonoBehaviour
{
    public bool hideModel = true;
    [SerializeField] bool hidePlayer;
    Collider player;
    public bool debug;
    void OnTriggerEnter(Collider col)
    {
        if(col.CompareTag("Player")) { player = col; hidePlayer = true; if(debug) Debug.LogWarning("Hiding the player"); }
    }

    void OnTriggerExit(Collider col)
    {
        if(col.CompareTag("Player"))
        {
            hidePlayer = false; 

            player.GetComponent<PlayerStats>().isHiding = false;
            if(hideModel)
            {
                player.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
                player.GetComponentInChildren<MeshRenderer>().enabled = true;
            }
            player = null;

            if(debug) Debug.LogWarning("No longerhiding the player");
        }
    }

    void Update()
    {
        if(player != null)
        {
            if(player.GetComponent<Movement>().animator.GetBool("isCrouching") && hidePlayer)
            {
                player.GetComponent<PlayerStats>().isHiding = true;
                if(hideModel)
                {
                    player.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
                    player.GetComponentInChildren<MeshRenderer>().enabled = false;
                }
            }
            else
            {
                player.GetComponent<PlayerStats>().isHiding = false;
                if(hideModel)
                {
                    player.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
                    player.GetComponentInChildren<MeshRenderer>().enabled = true;
                }
            }
        }
    }
}
