using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoppableObject : MonoBehaviour
{
   public Component[] scripts;
   public void TimeStopped()
   {
        if(TryGetComponent<Animator>(out Animator animator)) { animator.SetFloat("timeStopped", 0); }
        foreach (Component script in scripts) 
        {
            if (script is MonoBehaviour monoBehaviour)
            {
                monoBehaviour.enabled = false;
            }
        }
   }

    public void TimeUnfreezed()
    {
        if(TryGetComponent<Animator>(out Animator animator)) { animator.SetFloat("timeStopped", 1); }
        foreach (Component script in scripts) 
        {
            if (script is MonoBehaviour monoBehaviour)
            {
                monoBehaviour.enabled = true;
            }
        }
    }
}
