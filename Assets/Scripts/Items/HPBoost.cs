using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class HPBoost : MonoBehaviour
{
    public int hpboost = 15;
    
    public void ApplyEffect(GameObject player)
    {
        PlayerStats ps = player.gameObject.GetComponent<PlayerStats>();
        ps.health += hpboost;
        if(ps.health > 100) ps.health = 100;
        print($"Now you have {ps.health} HP!");
    }
}
