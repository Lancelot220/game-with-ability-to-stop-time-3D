using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthOnScreen : MonoBehaviour
{
    void Update()
    {
        GetComponent<Slider>().value = GameObject.FindWithTag("Player").GetComponent<PlayerStats>().health;
        /*if(GameObject.FindWithTag("Player").GetComponent<PlayerStats>().health < 1)
        {GameObject.Find("Health Fill Area").SetActive(false);}
        else {GameObject.Find("Health Fill Area").SetActive(true);}  */     
    }
}
