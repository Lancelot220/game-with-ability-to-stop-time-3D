using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;

public class EndScreen : MonoBehaviour
{
    [SerializeField] LocalizedString orbsLS;
    [SerializeField] TextMeshProUGUI orbsTMP;
    [SerializeField] LocalizedString timeLS;
    [SerializeField] TextMeshProUGUI timeTMP;

    void Start()
    {
        orbsLS.Arguments = new object[] { PlayerPrefs.GetInt("orbsCollected") };
        orbsLS.StringChanged += UpdateOrbs;


        float totalSeconds = PlayerPrefs.GetInt("time");
        // Обчислюємо години
        int hours = (int)(totalSeconds / 3600);
    
        // Залишок часу після обчислення годин
        totalSeconds %= 3600;
    
        // Обчислюємо хвилини
        int minutes = (int)(totalSeconds / 60);
    
        // Залишок часу після обчислення хвилин
        totalSeconds %= 60;
    
        // Обчислюємо секунди
        int seconds = (int)totalSeconds;
    
        // Обчислюємо мілісекунди
        int milliseconds = (int)((totalSeconds - seconds) * 1000);

        timeLS.Arguments = new object[] { hours, minutes, seconds, milliseconds };
        timeLS.StringChanged += UpdateTime;
    }

    void UpdateOrbs(string value) { orbsTMP.text = value; }
    void UpdateTime(string value) { timeTMP.text = value; }
}