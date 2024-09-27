using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class GPSettings : MonoBehaviour
{
    public void ChangeLang(int index)
    { StartCoroutine(ChangeLang_(index)); }
    IEnumerator ChangeLang_(int index)
    {
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
    }
}
