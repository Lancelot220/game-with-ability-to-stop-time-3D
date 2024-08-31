using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LeveEnd : MonoBehaviour
{
    public Scene nextLevel;
    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            print("Level Complete!");
            //SceneManager.LoadSceneAsync(nextLevel.buildIndex, LoadSceneMode.Additive);
        }
    }
}
