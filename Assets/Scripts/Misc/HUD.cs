using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Slider health;
    public TMP_Text orbsCount;
    public TMP_Text lives;
    PlayerStats ps;
    Movement m;
    Animator orbsAnimator;
    bool countUpdated;
    float showUpTimer;
    void Awake()
    {
        ps = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
        m = GameObject.FindWithTag("Player").GetComponent<Movement>();
        orbsAnimator = orbsCount.gameObject.GetComponent<Animator>();
    }
    void Update()
    {
        health.value = ps.health;

        if(orbsCount.text != Convert.ToString(ps.orbsCollected))
        {
            orbsCount.text = Convert.ToString(ps.orbsCollected);
            orbsAnimator.SetBool("showUp", true);
            countUpdated = true;
            showUpTimer = 5f;
        }
        else countUpdated = false;
/*
        if(lives.text != Convert.ToString(ps.Lives))
        {
            lives.text = Convert.ToString(ps.Lives);    
            lives.gameObject.GetComponent<Animator>().SetTrigger("showUp");
        }*/

        if(!countUpdated) showUpTimer -= Time.deltaTime;
        if(showUpTimer <= 0)
        {
            orbsAnimator.SetBool("showUp", false);
        }
    }
}
