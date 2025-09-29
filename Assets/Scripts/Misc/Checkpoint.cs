using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    /*[HideInInspector]*/ public int index;
    GameObject player;
    public bool achieved = true;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player") && !achieved && player.GetComponent<PlayerStats>().health > 0)
        { CheckpointAchieved(col.gameObject); print($"Checkpoint {index} achieved!"); }
    }

    public void CheckpointAchieved(GameObject player)
    {
        // PlayerPrefs.SetInt("lastCheckpoint", index);
        // PlayerPrefs.SetFloat("time", player.GetComponent<PlayerStats>().time);
        // PlayerPrefs.SetInt("orbsCollected", player.GetComponent<PlayerStats>().orbsCollected);
        var objectsInRange = new SavedObject[1];
        if (player.GetComponent<StopTime_>().objectsInRange != null)
        objectsInRange =
        player.GetComponent<StopTime_>().objectsInRange
            .Where(obj => obj != null && obj.gameObject != null)
            .Select(obj => new SavedObject(obj.gameObject))
            .ToArray();

        CheckpointData checkpoint = new CheckpointData(
            index,
            SceneManager.GetActiveScene().buildIndex,
            player.GetComponent<PlayerStats>().health,
            player.GetComponent<PlayerStats>().time,
            player.GetComponent<StopTime_>().cdTimer,
            player.GetComponent<StopTime_>().durationTimer,
            objectsInRange,
            //player.GetComponent<StopTime_>().objectsInRange.Select(obj => new SavedObject(obj.gameObject)).ToArray(),
            new float[] { player.GetComponent<StopTime_>().effectGO.transform.position.x,
                        player.GetComponent<StopTime_>().effectGO.transform.position.y,
                        player.GetComponent<StopTime_>().effectGO.transform.position.z },
            player.GetComponent<PlayerStats>().orbsCollected,
            player.GetComponent<PlayerStats>().unlockedSkills
        );

        GameObject.Find("Message:Checkpoint").GetComponent<Animator>().Play("ShowUp");
        SaveSystem.Save(null, new List<string>(), 0, new List<string>(), checkpoint);
        achieved = true;
    }
}
