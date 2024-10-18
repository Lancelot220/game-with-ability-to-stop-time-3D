using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class StopTimeCDBoost : MonoBehaviour
{
    public int cdBoost = 5;

    public void ApplyEffect(GameObject player)
    {
        StopTime_ st = player.gameObject.GetComponent<StopTime_>();
        if (!st.canStopTime && !st.timeStopped) 
        {
            st.cdTimer += cdBoost;
            if(st.cdTimer > st.cd) { st.cdTimer = st.cd; }
            else { print($"Now you have to wait only {st.cd - st.cdTimer} second(s)!"); }
        }
    }
}
