using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.InputSystem;

public class EndScreen : MonoBehaviour
{
    [SerializeField] LocalizedString orbsLS;
    [SerializeField] TextMeshProUGUI orbsTMP;
    [SerializeField] LocalizeStringEvent timeLSE;
    [SerializeField] TextMeshProUGUI timeTMP;
    public int orbsCollected;
    public int hours, minutes, seconds, milliseconds;

    void Awake()
    {
        //orbsLS.Arguments = new object[] {  };
        //orbsLS.RefreshString(); //StringChanged += UpdateOrbs;

        orbsCollected = PlayerPrefs.GetInt("orbsCollected");
        PlayerPrefs.SetInt("totalOrbs", PlayerPrefs.GetInt("totalOrbs") + orbsCollected);
        PlayerPrefs.SetInt("orbsCollected", 0);

        float totalSeconds = PlayerPrefs.GetFloat("time");
        // Обчислюємо години
        hours = (int)(totalSeconds / 3600);
    
        // Залишок часу після обчислення годин
        totalSeconds %= 3600;
    
        // Обчислюємо хвилини
        minutes = (int)(totalSeconds / 60);
    
        // Залишок часу після обчислення хвилин
        totalSeconds %= 60;
    
        // Обчислюємо секунди
        seconds = (int)totalSeconds;
    
        // Обчислюємо мілісекунди
        milliseconds = (int)((totalSeconds - seconds) * 1000);

        /*
        orbsLS.Arguments = new object[] { orbsCollected };
        orbsLS.RefreshString();

        timeLSE.StringReference.Arguments = new object[] { hours, minutes, seconds, milliseconds };
        timeLSE.RefreshString(); //StringChanged += UpdateTime;
        */

        PlayerPrefs.SetFloat("time", 0);
    }

    //void UpdateOrbs(string value) { orbsTMP.text = value; }
    //void UpdateTime(string value) { timeTMP.text = value; }
}