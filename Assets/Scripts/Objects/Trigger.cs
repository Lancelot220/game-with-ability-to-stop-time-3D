// using System.Collections;
// using System.Collections.Generic;
// using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;
using UnityEngine.Events;

public class Trigger : MonoBehaviour
{
    public UnityEvent onTriggerEnter;
    public bool multiTrigger = false;
    bool otenTriggered;
    public UnityEvent onTriggerStay;
    public UnityEvent onTriggerExit;
    public bool m_MultiTrigger = false;
    bool otexTriggered;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !otenTriggered)
        {
            onTriggerEnter.Invoke();
            if (!multiTrigger) otenTriggered = true;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            onTriggerStay.Invoke();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !otexTriggered)
        {
            onTriggerExit.Invoke();
            if (!m_MultiTrigger) otexTriggered = true;
        }
    }
}
