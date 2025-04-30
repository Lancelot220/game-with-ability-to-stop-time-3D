using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShowVersionInMenu : MonoBehaviour
{
    void Start()
    {
        GetComponent<TextMeshProUGUI>().text = "ver. " + Application.version;   
    }
}
