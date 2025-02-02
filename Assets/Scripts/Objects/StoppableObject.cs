using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoppableObject : MonoBehaviour
{
   public void TimeStopped()
   {
        if(TryGetComponent<Animator>(out Animator animator)) { animator.SetFloat("timeStopped", 0); }
   }

    public void TimeUnfreezed()
    {
          if(TryGetComponent<Animator>(out Animator animator)) { animator.SetFloat("timeStopped", 1); }
    }
}
