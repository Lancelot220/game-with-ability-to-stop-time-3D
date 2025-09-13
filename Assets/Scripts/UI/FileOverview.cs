using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FileOverview : MonoBehaviour
{
    public TextMeshProUGUI fileNameText;
    public int levelsCompleted;
    public int orbsCollected;
    public GameObject startButton;
    public GameObject continueButton;
    public int nextLevelIndex;

    void OnEnable()
    {
        if (SaveSystem.currentSaveData != null)
        {
            fileNameText.text = PlayerPrefs.GetString("SaveFileName", "Player");
            levelsCompleted = SaveSystem.currentSaveData.completedLevels.Count;
            orbsCollected = SaveSystem.currentSaveData.totalOrbsCollected;
        }
        else
        {
            levelsCompleted = 0;
            orbsCollected = 0;
            Menu.OpenPopup(GameObject.Find("No Save File Error").transform.GetChild(0).gameObject);
            GameObject.Find("No Save File Error").GetComponent<Image>().enabled = true;
            return;
        }

        if (levelsCompleted == 0 && SaveSystem.currentSaveData.checkpoint == null)
        {
            startButton.SetActive(true);
            continueButton.SetActive(false);
        }
        else
        {
            startButton.SetActive(false);
            continueButton.SetActive(true);

            nextLevelIndex = FindNextLevel();
        }
    }

    public static int FindNextLevel()
    {
        int levelIndex = 0; //a number from scene name
        foreach (string level in SaveSystem.currentSaveData.unlockedLevels)
        {
            string levelNum = level.Substring(3);
            if (float.TryParse(levelNum, out float f))
            {
                int index = (int)f;
                if (index > levelIndex)
                {
                    levelIndex = index;
                }
            }
        }
        if (levelIndex == 0)
        {
            levelIndex = 1; // if no levels unlocked, start from level 1
        }

        string sceneName = "Lvl" + levelIndex;
        int sceneCount = SceneManager.sceneCountInBuildSettings;

        int nextLevel = -1;
        for (int i = 0; i < sceneCount; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
            {
                //nextLevelIndex = i;
                nextLevel = i;
                //return;
            }
        }

        if (nextLevel == -1)
        {
            Debug.LogWarning("Scene with name " + sceneName + " not found in Build Settings.");
        }
        return nextLevel;
        //nextLevelIndex = -1; // або залиш як було, якщо сцени немає
    }

    public void LoadNextLevel()
    {
        GetComponent<LevelLoad>().LoadLevel(nextLevelIndex);
    }
}
