using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class GPSettings : MonoBehaviour
{
    //LANGUAGE
    public TMP_Dropdown langDropdown;
    public void ChangeLang(int index)
    { StartCoroutine(ChangeLang_(index)); }
    public IEnumerator ChangeLang_(int index)
    {
        yield return LocalizationSettings.InitializationOperation;
        if(index != -1)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
            PlayerPrefs.SetInt("selectedlanguageIndex", index);
        }
    }

    //GUIDE ALPHA
    public Toggle disableGuideToggle;

    public void DisableGuide(bool isOn)
    {
        if(isOn) PlayerPrefs.SetInt("disableGuide", 1);
        else PlayerPrefs.SetInt("disableGuide", 0);
    }
    void Awake()
    {
        //LANGUAGE
        
        var selectedLocale = LocalizationSettings.SelectedLocale;
        var locales = LocalizationSettings.AvailableLocales.Locales;
        int localeIndex = 0;
        for (int i = 0; i< locales.Count; i++) if (locales[i] == selectedLocale) localeIndex = i;
        langDropdown.value = localeIndex;
        StartCoroutine(ChangeLang_(PlayerPrefs.GetInt("selectedlanguageIndex", -1)));

        //GUIDE ALPHA
        if(PlayerPrefs.GetInt("disableGuide", 0) == 1) disableGuideToggle.isOn = true; 
        else disableGuideToggle.isOn = false;
    }
}
