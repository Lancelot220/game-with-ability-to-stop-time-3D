using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Linq;

public class SettingsTabChanging : MonoBehaviour
{
    public GameObject tabs;
    public GameObject nextTab;
    public void ChangeTab()
    {
        //Transform currentTab = tabs.transform.Cast<Transform>().FirstOrDefault(child => child.gameObject.activeSelf);
        //currentTab.gameObject.SetActive(false);

        for (int i = 0; i < tabs.transform.childCount; i++) tabs.transform.GetChild(i).gameObject.SetActive(false);
        nextTab.SetActive(true);
    }
}