using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Back : MonoBehaviour
{

    Controls ctrls;
    InputAction cancel;
    public GameObject currentScreen;
    public GameObject nextScreen;
    public Button nextScreenFirstButton;
    private Animator animator;
    public float animationDuration = 0.25f;
    public AudioSource sound;

    void Awake() { ctrls = new Controls(); }
    void OnEnable() { cancel = ctrls.UI.Cancel;
     cancel.Enable(); 
     cancel.performed += BackTo; }
    void OnDisable() { cancel.Disable(); }
    void BackTo(InputAction.CallbackContext context) { BackTo(); }
    
    public void BackTo()
    {
        animator = currentScreen.GetComponent<Animator>();
        animator.SetTrigger("BackTo");
        StartCoroutine(ChangeScreen());
        sound.Play();
    }
    
    IEnumerator ChangeScreen()
    {
        yield return new WaitForSecondsRealtime(animationDuration);

        currentScreen.SetActive(false);
        nextScreen.SetActive(true);
        nextScreenFirstButton.Select();
    }
}
