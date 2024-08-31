using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//using UnityEngine.UI;

public class LevelPortal : MonoBehaviour
{
    public int sceneIndex;
    public GameObject loadingScreen;

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

            loadingScreen.SetActive(true);
        }
    }
}
