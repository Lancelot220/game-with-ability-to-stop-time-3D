using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupClose : MonoBehaviour
{
    Animator animator;
    [Tooltip("Can be empty (Has null check).")] public Selectable selectAfterClosing;
    void Start() { animator = GetComponent<Animator>(); }

    public void Close()
    {
        animator.SetTrigger("Close");
        if(selectAfterClosing != null) selectAfterClosing.Select();
    }

    public void CloseEvent()
    {
        gameObject.SetActive(false);
    }
}
