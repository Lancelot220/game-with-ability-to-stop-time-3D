using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsInitializer : MonoBehaviour
{
    void Start()
    {
        GetComponentInChildren<SoundSettings>(true).SetSavedSettings();
        StartCoroutine(GetComponentInChildren<GPSettings>(true).ChangeLang_(PlayerPrefs.GetInt("selectedlanguageIndex", 0)));
    }
}
