using System.Collections;
using System.Collections.Generic;
//using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.SceneManagement;
//using UnityEngine.UI;

public class LevelLoad: MonoBehaviour
{
    public int scene;
    public GameObject currentScreen;
    public GameObject backScript;
    public GameObject loadingScreen;
    public float animationDuration = 0.75f;
    //ImageAnimation ia;
    [Tooltip("For Next level button on End level screen")] public bool nextLevel; 
    [Tooltip("Restart From Checkpoint")] public bool RFC;
    [Tooltip("Restart Entire Level")] public bool REL;
    void OnTriggerEnter(Collider col)
    { if (col.CompareTag("Player")){ LoadLevel(scene); } }

    public void LoadLevel(int sceneIndex)
    {
        currentScreen.GetComponent<Animator>().Play("LevelLoad");
        if (backScript != null) backScript.GetComponent<Back>().enabled = false;
        if(nextLevel)
        StartCoroutine(LoadScene(PlayerPrefs.GetInt("lastLevel") + 1));
        else if(RFC)
        StartCoroutine(LoadScene(PlayerPrefs.GetInt("lastLevel")));
        else if(REL)
        {
            StartCoroutine(LoadScene(PlayerPrefs.GetInt("lastLevel")));
            PlayerPrefs.SetInt("lastCheckpoint", 0);
        }
        else
        StartCoroutine(LoadScene(sceneIndex));
    }

    public void Restart(bool entireLevel)
    {
        currentScreen.GetComponent<Animator>().Play("LevelLoad");
        Time.timeScale = 1;
        if(entireLevel) PlayerPrefs.SetInt("lastCheckpoint", 0);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator LoadScene(int sceneIndex)
    {
        yield return new WaitForSecondsRealtime(animationDuration);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.priority = -1;
        //operation.allowSceneActivation = false;
        loadingScreen.SetActive(true);
        //StartCoroutine(LoadScene());
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
