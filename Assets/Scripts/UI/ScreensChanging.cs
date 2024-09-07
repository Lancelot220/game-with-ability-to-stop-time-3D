using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class ScreensChanging : MonoBehaviour
{
    public GameObject currentScreen;
    public GameObject nextScreen;
    public float animationDuration = 0.5f;
    Animator animator;
    public void GoTo()
    {
        animator = GetComponent<Animator>();
        animator.SetTrigger("GoTo");
        StartCoroutine(ChangeScreen());
    }

    public void BackTo()
    {
        animator = GetComponent<Animator>();
        animator.SetTrigger("BackTo");
        StartCoroutine(ChangeScreen());
    }

    IEnumerator ChangeScreen()
    {
        yield return new WaitForSeconds(animationDuration);

        currentScreen.SetActive(false);
        nextScreen.SetActive(true);
    }
}