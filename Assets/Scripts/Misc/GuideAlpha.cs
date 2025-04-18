using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GuideAlpha : MonoBehaviour
{
    public GameObject guideToShow;
    PlayerStats ps;
    bool guideWasShown;

    void Start()
    {
        if(PlayerPrefs.GetInt("disableGuide", 0) == 1) transform.parent.gameObject.SetActive(false);
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            ps = other.GetComponent<PlayerStats>();
            if(!guideWasShown)
            {
                guideToShow.SetActive(true);
                Time.timeScale = 0f;
            }
        }
    }

    void Update()
    {
        if(ps != null)
        {
            if(ps.isInteracting)
            {
                if(!guideWasShown)
                {
                    guideToShow.SetActive(false);
                    Time.timeScale = 1f;
                    ps.isInteracting = false;
                    guideWasShown = true;
                }
                else
                {
                    guideToShow.SetActive(true);
                    Time.timeScale = 0f;
                    ps.isInteracting = false;
                    guideWasShown = false;
                }
            }
        }   
    }
    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            //guideToShow.SetActive(false);
            ps = null;
        }
    }
}
