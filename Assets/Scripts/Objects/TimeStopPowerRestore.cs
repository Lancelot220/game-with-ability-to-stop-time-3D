using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeStopPowerRestore : MonoBehaviour
{
    public void RestorePower()
    {
        GameObject player = GameObject.Find("Player");
        StopTime_ st = player.GetComponent<StopTime_>();
        if(st.timeStopped)
        st.ForceUnfreezeTime();
    }
}
