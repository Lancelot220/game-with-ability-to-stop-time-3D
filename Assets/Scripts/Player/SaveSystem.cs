using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Diagnostics;
using UnityEditor.Localization.Plugins.XLIFF.V12;

public static class SaveSystem
{
    public static SaveData currentSaveData;
    public const string fileExtention = ".gwtst3d"; // change extension to actual game name when i'll have one 
    public static void Save(string level, List<string> unlockedLevels, int orbs, List<string> skills, CheckpointData checkpoint)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Path.Combine(Application.persistentDataPath, PlayerPrefs.GetString("SaveFileName", "player1") + fileExtention); // change extension to actual game name when i'll have one
        FileStream stream = new FileStream(path, FileMode.Create);

        if (currentSaveData == null)
        {
            currentSaveData = new SaveData(new List<string>(), new List<string>(), 0, new List<string>(), null);
        }
        else
        {
            if(!string.IsNullOrEmpty(level) && !currentSaveData.completedLevels.Contains(level)) currentSaveData.completedLevels.Add(level);
            if (unlockedLevels != null && unlockedLevels.Count > 0)
            {
                foreach (var unlockedLevel in unlockedLevels)
                {
                    if (!currentSaveData.unlockedLevels.Contains(unlockedLevel))
                    {
                        currentSaveData.unlockedLevels.Add(unlockedLevel);
                    }
                }
            }
            currentSaveData.totalOrbsCollected += orbs;
            foreach (string skill in skills)
            {
                if (!currentSaveData.unlockedSkills.Contains(skill))
                {
                    currentSaveData.unlockedSkills.Add(skill);
                }
            }
            currentSaveData.checkpoint = checkpoint;
        }
        SaveData data = currentSaveData;

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static void Load()
    {
        string path = Path.Combine(Application.persistentDataPath, PlayerPrefs.GetString("SaveFileName", "player1.gwtst3d") + fileExtention);
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            SaveData data = formatter.Deserialize(stream) as SaveData;
            stream.Close();
            currentSaveData = data;
        }
        else
        {
            UnityEngine.Debug.LogError("Save file not found in " + path);
        }
    }
}

[Serializable]
public class SaveData
{
    public List<string> completedLevels;
    public List<string> unlockedLevels;
    public int totalOrbsCollected;
    public List<string> unlockedSkills;
    public CheckpointData checkpoint;

    public SaveData(List<string> completedLevels, List<string> unlockedLevels, int totalOrbsCollected, List<string> unlockedSkills, CheckpointData checkpoint)
    {
        this.completedLevels = completedLevels;
        this.unlockedLevels = unlockedLevels;
        this.totalOrbsCollected = totalOrbsCollected;
        this.unlockedSkills = unlockedSkills;
        this.checkpoint = checkpoint;
    }
}

[Serializable]
public class CheckpointData
{
    public int checkpointIndex;
    public int level;
    public int health;
    public float time;
    public float timeStopCD;
    public float durationTimer;
    public SavedObject[] stoppedObjects;
    public float[] stopSpherePosition;

    public int orbsCollected;
    public List<string> unlockedSkills;

    public CheckpointData(int checkpointIndex, int level, int health, float time, float timeStopCD, float durationTimer, SavedObject[] stoppedObjects, float[] stopSpherePosition, int orbsCollected, List<string> skillsUnlocked)
    {
        this.checkpointIndex = checkpointIndex;
        this.level = level;
        this.health = health;
        this.time = time;
        this.timeStopCD = timeStopCD;
        this.durationTimer = durationTimer;
        this.stoppedObjects = stoppedObjects;
        this.stopSpherePosition = stopSpherePosition;

        this.orbsCollected = orbsCollected;
        this.unlockedSkills = skillsUnlocked;
    }
}

[Serializable]
public class SavedObject
{
    public string id;
    public float[] position;
    public float[] rotation;

    public SavedObject(GameObject obj)
    {
        if (obj.GetComponent<UniqueID>() == null)
        {
            UnityEngine.Debug.LogWarning(obj.name + " was ignored because it does not have a UniqueID component.");
            return;
        }
        id = obj.GetComponent<UniqueID>().uniqueID;
        position = new float[3] { obj.transform.position.x, obj.transform.position.y, obj.transform.position.z };
        rotation = new float[4] { obj.transform.rotation.x, obj.transform.rotation.y, obj.transform.rotation.z, obj.transform.rotation.w };
    }
}
