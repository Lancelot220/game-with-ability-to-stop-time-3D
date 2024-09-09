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
            StartCoroutine(EndLevelScene());
        }
    }

    IEnumerator EndLevelScene()
    {
        yield return new WaitForSeconds(animationDuration);

        PlayerPrefs.SetInt("lastLevel",SceneManager.GetActiveScene().buildIndex);
        SceneManager.LoadScene("End Level");
    }
}
