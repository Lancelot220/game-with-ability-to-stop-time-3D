using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartlyStoppable : MonoBehaviour
{
    public Component[] scripts;
    public Component timeStopScript;
    void Update()
    {
        bool timeStopped = (bool)timeStopScript.GetType().GetField("timeStopped").GetValue(timeStopScript);
        if(timeStopped)
        {
            foreach(Component script in scripts)
            {
                script.GetType().GetField("timeStopped").SetValue(script, false);
            }
        }
    }
}
