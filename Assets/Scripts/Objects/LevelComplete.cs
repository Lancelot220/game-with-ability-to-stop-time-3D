using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelComplete : MonoBehaviour
{
    public GameObject currentScreen;
    public float animationDuration = 0.75f;
    void OnTriggerEnter(Collider col)
    {
        if(col.CompareTag("Player"))
        {
            currentScreen.GetComponent<Animator>().SetTrigger("LevelLoad");
            StartCoroutine(EndLevelScene(col.gameObject));
            PlayerPrefs.SetInt("lastCheckpoint", 0);
        }
    }

    IEnumerator EndLevelScene(GameObject player)
    {
        yield return new WaitForSeconds(animationDuration);

        PlayerPrefs.SetInt("lastLevel",SceneManager.GetActiveScene().buildIndex);
        PlayerPrefs.SetFloat("time", player.GetComponent<PlayerStats>().time);
        PlayerPrefs.SetInt("orbsCollected", player.GetComponent<PlayerStats>().orbsCollected);
        SceneManager.LoadScene("End Level");
    }
}
