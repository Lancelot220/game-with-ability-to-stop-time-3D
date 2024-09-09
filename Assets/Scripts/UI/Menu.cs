using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void BackTo(bool toMainMenu)
    {
        animator = currentScreen.GetComponent<Animator>();
        animator.SetTrigger("BackTo");
        if (!toMainMenu)
        StartCoroutine(ChangeScreen());
        else
        StartCoroutine(BackToMainMenu());
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

    IEnumerator BackToMainMenu()
    {
        yield return new WaitForSeconds(animationDuration);

        SceneManager.LoadScene("Main Menu");
    }
}