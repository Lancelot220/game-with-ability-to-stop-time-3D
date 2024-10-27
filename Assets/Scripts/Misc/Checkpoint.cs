using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [HideInInspector] public int index;
    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        { CheckpointAchieved(); }
    }

    public void CheckpointAchieved()
    {
        PlayerPrefs.SetInt("lastCheckpoint", index);
    }
}
