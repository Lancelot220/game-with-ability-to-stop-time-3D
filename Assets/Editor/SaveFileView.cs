#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

public class SaveFileView : EditorWindow
{
    enum Slot { First, Second, Third, Fourth }
    Slot slot;
    SaveData currentSaveData;
    bool fileLoaded;
    bool addCheckpointData;
    Vector2 scrollPos;

    [MenuItem("Window/Save File View")]
    public static void ShowWindow()
    {
        var window = GetWindow<SaveFileView>("Save File View");

        window.titleContent = new GUIContent("Save File View", EditorGUIUtility.IconContent("SaveAs").image);

    }

    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));
        
        slot = (Slot)EditorGUILayout.EnumPopup("Save Slot", slot);

        if (slot == Slot.First)
        {
            PlayerPrefs.SetString("SaveFileName", PlayerPrefs.GetString("SaveFile1", ""));
            //fileLoaded = false;
            //currentSaveData = null;
        }
        else if (slot == Slot.Second)
        {
            PlayerPrefs.SetString("SaveFileName", PlayerPrefs.GetString("SaveFile2", ""));
            //fileLoaded = false;
            //currentSaveData = null;
        }
        else if (slot == Slot.Third)
        {
            PlayerPrefs.SetString("SaveFileName", PlayerPrefs.GetString("SaveFile3", ""));
            //fileLoaded = false;
            //currentSaveData = null;
        }
        else if (slot == Slot.Fourth)
        {
            PlayerPrefs.SetString("SaveFileName", PlayerPrefs.GetString("SaveFile4", ""));
            //fileLoaded = false;
            //currentSaveData = null;
        }

        if (GUILayout.Button("Load")) Load();

        GUILayout.Space(10);
        GUILayout.Label("Save data", EditorStyles.boldLabel);
        if (currentSaveData != null)
        {
            GUILayout.Label("Completed Levels: " + string.Join(", ", currentSaveData.completedLevels));
            GUILayout.Label("Unlocked Levels: " + string.Join(", ", currentSaveData.unlockedLevels));
            GUILayout.Label("Total Orbs Collected: " + currentSaveData.totalOrbsCollected);
            GUILayout.Label("Unlocked Skills: " + string.Join(", ", currentSaveData.unlockedSkills));
            GUILayout.Label("Checkpoint data", EditorStyles.boldLabel);
            if (currentSaveData.checkpoint != null)
            {
                GUILayout.Label("Checkpoint Index: " + currentSaveData.checkpoint.checkpointIndex);
                GUILayout.Label("Level: " + currentSaveData.checkpoint.level);
                GUILayout.Label("Health: " + currentSaveData.checkpoint.health);
                GUILayout.Label("Orbs: " + currentSaveData.checkpoint.orbsCollected);
                GUILayout.Label("Time: " + currentSaveData.checkpoint.time);
                GUILayout.Label("TimeStop Cooldown: " + currentSaveData.checkpoint.timeStopCD);
                GUILayout.Label("Duration timer: " + currentSaveData.checkpoint.durationTimer);
                GUILayout.Label("Stopped objects: " + currentSaveData.checkpoint.stoppedObjects.Length);
                GUILayout.Label("Stop Sphere Position: " + (currentSaveData.checkpoint.stopSpherePosition != null ? string.Join(", ", currentSaveData.checkpoint.stopSpherePosition) : "None"));
                string skills = "None";
                if (currentSaveData.checkpoint.unlockedSkills != null && currentSaveData.checkpoint.unlockedSkills.Count > 0)
                    skills = string.Join(", ", currentSaveData.checkpoint.unlockedSkills);
                GUILayout.Label("Unlocked Skills: " + skills);

            }
            else
            {
                GUILayout.Label("No Checkpoint Data");
            }
        }
        else
        {
            if (fileLoaded) GUILayout.Label("No save data found for this slot.");
            else GUILayout.Label("Press Load to view save data.");
        }

        GUILayout.Space(20);
        GUILayout.Label("Manual Save", EditorStyles.boldLabel);
        string level = EditorGUILayout.TextField("Level Name", "");
        List<string> completedLevels = new List<string>(EditorGUILayout.TextField("Completed Levels (,)", "").Split(','));
        List<string> unlockedLevels = new List<string>(EditorGUILayout.TextField("Unlocked Levels (,)", "").Split(','));
        int totalOrbs = EditorGUILayout.IntField("Total Orbs Collected", 0);
        List<string> unlockedSkills = new List<string>(EditorGUILayout.TextField("Unlocked Skills (,)", "").Split(','));
        addCheckpointData = EditorGUILayout.Toggle("Add Checkpoint Data", addCheckpointData);
        if (addCheckpointData)
        {
            GUILayout.Label("Checkpoint Data", EditorStyles.boldLabel);

            int checkpointIndex = EditorGUILayout.IntField("Checkpoint Index", 0);
            int levelIndex = EditorGUILayout.IntField("Level", 0);
            int health = EditorGUILayout.IntField("Health", 100);
            int orbsCollected = EditorGUILayout.IntField("Orbs Collected", 0);
            float time = EditorGUILayout.FloatField("Time", 0f);
            float timeStopCD = EditorGUILayout.FloatField("TimeStop Cooldown", 0f);
            float durationTimer = EditorGUILayout.FloatField("Duration Timer", 0f);
            List<string> checkpointSkills = new List<string>(EditorGUILayout.TextField("Unlocked Skills (,)", "").Split(','));
            float[] stopSpherePosition = new float[3];
            stopSpherePosition[0] = EditorGUILayout.FloatField("Stop Sphere Position X", 0f);
            stopSpherePosition[1] = EditorGUILayout.FloatField("Stop Sphere Position Y", 0f);
            stopSpherePosition[2] = EditorGUILayout.FloatField("Stop Sphere Position Z", 0f);

            if (GUILayout.Button("Save with Checkpoint"))
            {
                CheckpointData checkpoint = new CheckpointData
                (
                    checkpointIndex,
                    levelIndex,
                    health,
                    time,
                    timeStopCD,
                    durationTimer,
                    null,
                    stopSpherePosition,
                    orbsCollected,
                    checkpointSkills
                );
                SaveSystem.Save(level, unlockedLevels, totalOrbs, unlockedSkills, checkpoint);
                Load();
            }
        }
        else
        {
            if (GUILayout.Button("Save without Checkpoint"))
            {
                SaveSystem.Save(level, unlockedLevels, totalOrbs, unlockedSkills, null);
            }
        }
        GUILayout.Space(20);
        if(GUILayout.Button("Call SaveSystem.Load()")) SaveSystem.Load();

        EditorGUILayout.EndScrollView();
        
    }
    void Load()
    {
        string path = Path.Combine(Application.persistentDataPath, PlayerPrefs.GetString("SaveFileName", "player1.gwtst3d") + SaveSystem.fileExtention);
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            SaveData data = formatter.Deserialize(stream) as SaveData;
            stream.Close();
            currentSaveData = data;
            fileLoaded = true;
        }
        else
        {
            UnityEngine.Debug.LogError("Save file not found in " + path);
            currentSaveData = null;
            //fileLoaded = false;
        }
    }
}
#endif