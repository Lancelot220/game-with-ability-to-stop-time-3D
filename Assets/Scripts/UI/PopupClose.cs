using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupCloce : MonoBehaviour
{
    Animator animator;
    void Start() { animator = GetComponent<Animator>(); }

    public void Close()
    {
        animator.SetTrigger("Close");
    }

    public void CloseEvent()
    {
        gameObject.SetActive(false);
    }
}
