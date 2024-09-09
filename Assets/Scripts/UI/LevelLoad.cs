using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//using UnityEngine.UI;

public class LevelLoad: MonoBehaviour
{
    public int scene;
    public GameObject currentScreen;
    public GameObject loadingScreen;
    public float animationDuration = 0.75f;
    //ImageAnimation ia;
    public bool nextLevel, playAgain;
    void OnTriggerEnter(Collider col)
    { if (col.CompareTag("Player")){ LoadLevel(scene); } }

    public void LoadLevel(int sceneIndex)
    {
        currentScreen.GetComponent<Animator>().SetTrigger("LevelLoad");
        if(nextLevel)
        StartCoroutine(LoadScene(PlayerPrefs.GetInt("lastLevel") + 1));
        else if(playAgain)
        StartCoroutine(LoadScene(PlayerPrefs.GetInt("lastLevel")));
        else
        StartCoroutine(LoadScene(sceneIndex));
    }

    IEnumerator LoadScene(int sceneIndex)
    {
        yield return new WaitForSeconds(animationDuration);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        //operation.allowSceneActivation = false;
        loadingScreen.SetActive(true);
        //StartCoroutine(LoadScene());
    }
}
