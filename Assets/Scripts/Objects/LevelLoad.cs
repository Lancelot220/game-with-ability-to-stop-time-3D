using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//using UnityEngine.UI;

public class LevelLoad: MonoBehaviour
{
    public int sceneIndex;
    public GameObject loadingScreen;
    public float loadDelay = 3;
    //ImageAnimation ia;

    void OnTriggerEnter(Collider col)
    { if (col.CompareTag("Player")){ LoadLevel(); } }

    void LoadLevel()
    { 
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        //operation.allowSceneActivation = false;
        loadingScreen.SetActive(true);
        //StartCoroutine(LoadScene());
    }
/*
    IEnumerator LoadScene()
    {
        yield return null;

        
        
        ia = GameObject.Find("Loading Animation").GetComponent<ImageAnimation>();
        while(!operation.isDone)
        {
            if(operation.progress >= 0.9f)
            {
                ia.enabled = false;
                yield return new WaitForSeconds(loadDelay);
                
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }*/
}
