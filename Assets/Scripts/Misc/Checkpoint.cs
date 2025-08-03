using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    [HideInInspector] public int index;
    GameObject player;
    bool achieved;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        { CheckpointAchieved(col.gameObject); print($"Checkpoint {index} achieved!"); }
    }

    public void CheckpointAchieved(GameObject player)
    {
        // PlayerPrefs.SetInt("lastCheckpoint", index);
        // PlayerPrefs.SetFloat("time", player.GetComponent<PlayerStats>().time);
        // PlayerPrefs.SetInt("orbsCollected", player.GetComponent<PlayerStats>().orbsCollected);

        CheckpointData checkpoint = new CheckpointData(
            index,
            SceneManager.GetActiveScene().buildIndex,
            player.GetComponent<PlayerStats>().health,
            player.GetComponent<StopTime_>().cdTimer,
            //player.GetComponent<StopTime_>().objectsInRange.Select(obj => new SavedObject(obj.gameObject)).ToArray(),
            player.GetComponent<StopTime_>().objectsInRange
            .Where(obj => obj != null && obj.gameObject != null)
            .Select(obj => new SavedObject(obj.gameObject))
            .ToArray(),

            new float[] { player.GetComponent<StopTime_>().effectGO.transform.position.x,
                        player.GetComponent<StopTime_>().effectGO.transform.position.y,
                        player.GetComponent<StopTime_>().effectGO.transform.position.z },
            player.GetComponent<PlayerStats>().orbsCollected,
            player.GetComponent<PlayerStats>().skillsUnlocked
        );

        GameObject.Find("Message:Checkpoint").GetComponent<Animator>().Play("ShowUp");
        SaveSystem.Save(null, new List<string>(), 0, new List<string>(), checkpoint);
        achieved = true;
    }
}
