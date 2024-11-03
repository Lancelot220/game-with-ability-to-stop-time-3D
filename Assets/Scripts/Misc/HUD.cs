using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Slider health;
    public TMP_Text orbsCount;
    public TMP_Text lives;
    PlayerStats ps;
    Movement m;
    void Awake()
    {
        ps = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
        m = GameObject.FindWithTag("Player").GetComponent<Movement>();
    }
    void Update()
    {
        health.value = ps.health;

        if(orbsCount.text != Convert.ToString(ps.orbsCollected))
        {
            orbsCount.text = Convert.ToString(ps.orbsCollected);
            orbsCount.gameObject.GetComponent<Animator>().SetTrigger("showUp");
        }
/*
        if(lives.text != Convert.ToString(ps.Lives))
        {
            lives.text = Convert.ToString(ps.Lives);    
            lives.gameObject.GetComponent<Animator>().SetTrigger("showUp");
        }*/
    }
}
