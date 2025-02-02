using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetBreakables : MonoBehaviour
{
   public GameObject[] breakablesToReset;

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            ResetBreakables_();
        }
    }

    public void ResetBreakables_()
    {
        foreach (GameObject breakable in breakablesToReset)
        {
            breakable.SetActive(true);
        }
    }
}
