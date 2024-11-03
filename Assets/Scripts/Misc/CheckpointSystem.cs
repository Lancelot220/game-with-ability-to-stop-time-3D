using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointSystem : MonoBehaviour
{
    Checkpoint[] checkpoints;
    GameObject player;
    //PlayerStats ps;
    void Start()
    {
        player = GameObject.Find("Player");
        //ps = player.GetComponent<PlayerStats>();
        checkpoints = GetComponentsInChildren<Checkpoint>();
        for (int i = 0; i < checkpoints.Length; i++) checkpoints[i].index = i;
        int cpIndex = PlayerPrefs.GetInt("lastCheckpoint");
        Transform lastCheckpoint = checkpoints[cpIndex].gameObject.transform;
        if(cpIndex != 0 && PlayerPrefs.GetInt("lastLevel") == SceneManager.GetActiveScene().buildIndex /*&& ps.Lives > 0*/)
        {
            player.transform.position = lastCheckpoint.position;
            player.transform.rotation = lastCheckpoint.rotation;
            //ps.Lives--;
            //PlayerPrefs.SetInt("lives", ps.lives); 
        }
        PlayerPrefs.SetInt("lastLevel",SceneManager.GetActiveScene().buildIndex);
    }
}
