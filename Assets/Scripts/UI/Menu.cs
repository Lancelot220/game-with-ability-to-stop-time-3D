using System.Collections;
using System.Collections.Generic;
//using Unity.PlasticSCM.Editor.WebApi;
//using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public GameObject currentScreen;
    public GameObject nextScreen;
    public Selectable nextScreenFirstButton;
    public float animationDuration = 0.25f;
    public bool useTransition;
    public bool isInMainMenu;
    Animator animator;
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if(isInMainMenu && PlayerPrefs.GetInt("BackToMainMenu", 0) == 1)
        {
            PlayerPrefs.SetInt("BackToMainMenu", 0);
            StartCoroutine(PlayOnNextFrame());
        }
    }
    IEnumerator PlayOnNextFrame() { yield return null; transform.parent.gameObject.GetComponent<Animator>().Play("Enter"); }
    public void GoTo()
    {
        animator = currentScreen.GetComponent<Animator>();
        animator.Play("ScreenChange"); //SetTrigger("GoTo");
        StartCoroutine(ChangeScreen());
    }

    public void BackTo(bool toMainMenu)
    {
        animator = currentScreen.GetComponent<Animator>();
        if(!useTransition) animator.Play("Back"); else animator.Play("LevelLoad");
        if (!toMainMenu)
        StartCoroutine(ChangeScreen());
        else
        {
            Time.timeScale = 1;
            StartCoroutine(BackToMainMenu());
            //GetComponent<LevelLoad>().LoadLevel(0); 
            Movement m = GameObject.Find("Player").GetComponent<Movement>(); 
            if(m != null) m.enabled = false;
            PlayerStats ps = GameObject.Find("Player").GetComponent<PlayerStats>();
            if(ps != null) ps.enabled = false;
            print("TRYING TO EEEXXXIIITTT");
        }
    }

    public void Exit()
    {
        print("GET OUT");
        Application.Quit();
    }

    IEnumerator ChangeScreen()
    {
        yield return new WaitForSecondsRealtime(animationDuration);

        currentScreen.SetActive(false);
        nextScreen.SetActive(true);
        nextScreen.GetComponent<Animator>().Play("Enter");
        //if(nextScreenFirstButton != null) nextScreenFirstButton.Select();
    }

    IEnumerator BackToMainMenu()
    {
        yield return new WaitForSecondsRealtime(animationDuration);
        //Time.timeScale = 1;
        print("EEEXXXIIITTT");
        PlayerPrefs.SetInt("BackToMainMenu", 1);
        SceneManager.LoadScene(0);
    }

    public void OpenPopup(GameObject popup)
    {
        popup.SetActive(true);
        popup.GetComponentInChildren<Animator>().Play("Open");
        popup.GetComponentInChildren<Button>().Select();
    }
}