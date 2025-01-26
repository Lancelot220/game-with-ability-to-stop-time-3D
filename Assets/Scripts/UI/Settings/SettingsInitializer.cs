using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsInitializer : MonoBehaviour
{
    void Start()
    {
        GetComponentInChildren<SoundSettings>(true).SetSavedSettings();
    }
}
