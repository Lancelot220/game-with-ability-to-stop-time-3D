using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class GPSettings : MonoBehaviour
{
    //LANGUAGE
    public TMP_Dropdown langDropdown;
    public void ChangeLang(int index)
    { StartCoroutine(ChangeLang_(index)); }
    IEnumerator ChangeLang_(int index)
    {
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        PlayerPrefs.SetInt("selectedlanguageIndex", index);
    }
    void Awake()
    {
        //LANGUAGE
        StartCoroutine(ChangeLang_(PlayerPrefs.GetInt("selectedlanguageIndex")));
        var selectedLocale = LocalizationSettings.SelectedLocale;
        var locales = LocalizationSettings.AvailableLocales.Locales;
        int localeIndex = 0;
        for (int i = 0; i< locales.Count; i++) if (locales[i] == selectedLocale) localeIndex = i;
        langDropdown.value = localeIndex;
    }
}
