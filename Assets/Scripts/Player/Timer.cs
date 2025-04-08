using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public float time;
    public TextMeshProUGUI timer;
    public bool runTimer = true;
    [Header("")]
    public int hours, minutes, seconds, milliseconds;
    PlayerStats ps;
    float maxTime;
    void Start() { ps = GetComponent<PlayerStats>(); maxTime = time; }
    void Update()
    {
        if(runTimer) time = maxTime - ps.time;
        if(time <= 0)
        {
            ps.health = 0;
        }

        float totalSeconds = time;
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

        if(timer != null) timer.text = string.Format("{0:D2}:{1:D2}.{2:D3}", minutes, seconds, milliseconds);
    }
}
