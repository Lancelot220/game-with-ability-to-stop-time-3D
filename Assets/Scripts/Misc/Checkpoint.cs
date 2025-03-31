using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [HideInInspector] public int index;
    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        { CheckpointAchieved(col.gameObject); print($"Checkpoint {index} achieved!"); }
    }

    public void CheckpointAchieved(GameObject player)
    {
        PlayerPrefs.SetInt("lastCheckpoint", index);
        PlayerPrefs.SetFloat("time", player.GetComponent<PlayerStats>().time);
        PlayerPrefs.SetInt("orbsCollected", player.GetComponent<PlayerStats>().orbsCollected);
    }
}
