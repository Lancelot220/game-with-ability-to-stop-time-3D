using System.Collections;
using System.Collections.Generic;
//using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillItem : MonoBehaviour
{
    PlayerStats playerStats;
    public string skillName;
    public GameObject skillDesc;
    GameObject descOnScene;
    public GameObject button;
    GameObject buttonOnScene;
    void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
        descOnScene = Instantiate(skillDesc, transform.position + Vector3.up * 2, Quaternion.identity);
        descOnScene.SetActive(false);
        descOnScene.transform.SetParent(GameObject.Find("HUD").transform);
        descOnScene.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        descOnScene.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        descOnScene.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        descOnScene.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);
        buttonOnScene = Instantiate(button, transform.position, Quaternion.identity);
        buttonOnScene.transform.SetParent(descOnScene.transform);
        buttonOnScene.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -350);
        buttonOnScene.SetActive(false);
    }
    public void UnlockSkill()
    {
        descOnScene.SetActive(true);
        buttonOnScene.SetActive(true);
        EventSystem.current.SetSelectedGameObject(buttonOnScene);
        GameObject.Find("HUD").GetComponent<Animator>().enabled = false;
        GameObject.Find("Black Screen").GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0.5f);
        Time.timeScale = 0f;
        if (playerStats != null && !playerStats.unlockedSkills.Contains(skillName))
        {
            playerStats.unlockedSkills.Add(skillName);
            Debug.Log($"Skill '{skillName}' unlocked!");
        }
        else
        {
            Debug.LogWarning("PlayerStats component not found or skill already unlocked.");
        }
    }
}
