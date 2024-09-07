using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public GameObject currentScreen;
    public GameObject nextScreen;
    public float animationDuration = 0.25f;
    Animator animator;
    public void GoTo()
    {
        animator = currentScreen.GetComponent<Animator>();
        animator.SetTrigger("GoTo");
        StartCoroutine(ChangeScreen());
    }

    public void BackTo()
    {
        animator = currentScreen.GetComponent<Animator>();
        animator.SetTrigger("BackTo");
        StartCoroutine(ChangeScreen());
    }

    public void Exit()
    {
        Application.Quit();
    }

    IEnumerator ChangeScreen()
    {
        yield return new WaitForSeconds(animationDuration);

        currentScreen.SetActive(false);
        nextScreen.SetActive(true);
    }
}