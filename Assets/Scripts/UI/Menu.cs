using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public GameObject currentScreen;
    public GameObject nextScreen;
    public Button nextScreenFirstButton;
    public float animationDuration = 0.25f;
    public bool useTransition;
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
        if(!useTransition) animator.SetTrigger("BackTo"); else animator.SetTrigger("LevelLoad");
        if (!toMainMenu)
        StartCoroutine(ChangeScreen());
        else
        {
            StartCoroutine(BackToMainMenu()); 
            Movement m = GameObject.Find("Player").GetComponent<Movement>(); 
            if(m != null) m.enabled = false;
        }
    }

    public void Exit()
    {
        Application.Quit();
    }

    IEnumerator ChangeScreen()
    {
        yield return new WaitForSecondsRealtime(animationDuration);

        currentScreen.SetActive(false);
        nextScreen.SetActive(true);
        nextScreenFirstButton.Select();
    }

    IEnumerator BackToMainMenu()
    {
        yield return new WaitForSecondsRealtime(animationDuration);

        SceneManager.LoadScene(0);
        Time.timeScale = 1;
    }

    public void OpenPopup(GameObject popup)
    {
        popup.SetActive(true);
        popup.GetComponentInChildren<Animator>().SetTrigger("Open");
        popup.GetComponentInChildren<Button>().Select();
    }
}