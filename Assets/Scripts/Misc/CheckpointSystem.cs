using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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

        SaveSystem.Load();
        if (SaveSystem.currentSaveData.checkpoint != null && SaveSystem.currentSaveData.checkpoint.level != SceneManager.GetActiveScene().buildIndex)
        {
            SaveSystem.currentSaveData.checkpoint = null;
        }

        int cpIndex = 0;
        if (SaveSystem.currentSaveData.checkpoint != null)
        cpIndex = SaveSystem.currentSaveData.checkpoint.checkpointIndex; //PlayerPrefs.GetInt("lastCheckpoint");
        

        // if (cpIndex > checkpoints.Length - 1 || cpIndex < 0) cpIndex = 0;
        Transform lastCheckpoint = checkpoints[cpIndex].gameObject.transform;
        if (cpIndex != 0 /*&& PlayerPrefs.GetInt("lastLevel") == SceneManager.GetActiveScene().buildIndex*/ /*&& ps.Lives > 0*/)
        {
            player.transform.position = lastCheckpoint.position;
            player.transform.rotation = lastCheckpoint.rotation;
            player.GetComponent<PlayerStats>().time = SaveSystem.currentSaveData.checkpoint.time; //PlayerPrefs.GetFloat("time");
            player.GetComponent<PlayerStats>().orbsCollected = SaveSystem.currentSaveData.totalOrbsCollected; //PlayerPrefs.GetInt("orbsCollected");

            player.GetComponent<PlayerStats>().health = SaveSystem.currentSaveData.checkpoint.health;
            player.GetComponent<PlayerStats>().skillsUnlocked = SaveSystem.currentSaveData.checkpoint.skillsUnlocked;
            player.GetComponent<StopTime_>().cdTimer = SaveSystem.currentSaveData.checkpoint.timeStopCD;

            if (SaveSystem.currentSaveData.checkpoint.stoppedObjects.Length > 0)
            {
                var stopTime = player.GetComponent<StopTime_>();
                var stoppedObjects = SaveSystem.currentSaveData.checkpoint.stoppedObjects;
                stopTime.objectsInRange = new Collider[stoppedObjects.Length];

                // Шукаємо всі об'єкти з UniqueID лише один раз
                UniqueID[] allUniqueObjects = GameObject.FindObjectsOfType<UniqueID>();

                for (int i = 0; i < stoppedObjects.Length; i++)
                {
                    string id = stoppedObjects[i].id;

                    // Шукаємо об'єкт з потрібним ID
                    UniqueID match = allUniqueObjects.FirstOrDefault(obj => obj.uniqueID == id);

                    if (match != null)
                    {
                        GameObject obj = match.gameObject;
                        Collider col = obj.GetComponent<Collider>();

                        if (col != null)
                        {
                            stopTime.objectsInRange[i] = col;
                            obj.SetActive(true);

                            // Відновлення позиції
                            if (stoppedObjects[i].position != null && stoppedObjects[i].position.Length == 3)
                                obj.transform.position = new Vector3(
                                    stoppedObjects[i].position[0],
                                    stoppedObjects[i].position[1],
                                    stoppedObjects[i].position[2]);

                            // Відновлення обертання
                            if (stoppedObjects[i].rotation != null && stoppedObjects[i].rotation.Length == 4)
                                obj.transform.rotation = new Quaternion(
                                    stoppedObjects[i].rotation[0],
                                    stoppedObjects[i].rotation[1],
                                    stoppedObjects[i].rotation[2],
                                    stoppedObjects[i].rotation[3]);
                        }
                    }
                    // else
                    // {
                    //     Debug.LogWarning($"Об'єкт з ID {id} не знайдено серед унікальних об'єктів.");
                    // }
                }
                stopTime.ForceStopTime(SaveSystem.currentSaveData.checkpoint.stopSpherePosition);
            }



            //ps.Lives--;
            //PlayerPrefs.SetInt("lives", ps.lives); 
        }
        PlayerPrefs.SetInt("lastLevel",SceneManager.GetActiveScene().buildIndex);
    }
}
