using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        }

        if (levelsCompleted == 0)
        {
            startButton.SetActive(true);
            continueButton.SetActive(false);
        }
        else
        {
            startButton.SetActive(false);
            continueButton.SetActive(true);

            FindNextLevel();
        }
    }

    //make it find which level is next to play
    void FindNextLevel()
    {
        int levelIndex = 0;
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

        string sceneName = "Lvl" + levelIndex;
        int sceneCount = SceneManager.sceneCountInBuildSettings;

        for (int i = 0; i < sceneCount; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
            {
                nextLevelIndex = i;
                return;
            }
        }

        Debug.LogWarning("Scene with name " + sceneName + " not found in Build Settings.");
        nextLevelIndex = -1; // або залиш як було, якщо сцени немає
    }

    public void LoadNextLevel()
    {
        GetComponent<LevelLoad>().LoadLevel(nextLevelIndex);
    }
}
