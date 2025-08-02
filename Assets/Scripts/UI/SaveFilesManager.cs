using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.UI;
//using System.IO;

public class SaveFilesManager : MonoBehaviour
{
    public int slotIndex;
    public string fileName;

    public TextMeshProUGUI text;
    public TextMeshProUGUI lastSaveText;
    public GameObject fileMananagementButtons;
    public GameObject fileCreationButtons;

    void OnEnable()
    {
        if (PlayerPrefs.GetString("SaveFile" + slotIndex, "") != "")
        {
            FileExists();
        }
        else
        {
            FileDoesntExist();
        }
    }
    void FileExists()
    {
        fileName = PlayerPrefs.GetString("SaveFile" + slotIndex);
        text.text = fileName;
        fileMananagementButtons.SetActive(true);
        fileCreationButtons.SetActive(false);
        lastSaveText.text = File.GetLastWriteTime(Path.Combine(Application.persistentDataPath, fileName + SaveSystem.fileExtention)).ToString("dd.MM.yyyy HH:mm:ss");
    }

    void FileDoesntExist()
    {
        fileCreationButtons.SetActive(true);
        fileMananagementButtons.SetActive(false);
    }

    public void CreateFile(string name)
    {
        if (name == "")
        {
            name = "player" + slotIndex;
        }
        fileName = name;
        PlayerPrefs.SetString("SaveFile" + slotIndex, fileName);
        PlayerPrefs.SetString("SaveFileName", fileName);
        SaveSystem.Save(null, new List<string>(), 0, new List<string>(), null); // Initial save with default values
        FileExists();
    }

    public void EditName(string newName)
    {
        if (newName == "")
        {
            newName = "player" + slotIndex;
        }
        File.Move(Path.Combine(Application.persistentDataPath, fileName + SaveSystem.fileExtention), Path.Combine(Application.persistentDataPath, newName + SaveSystem.fileExtention));
        PlayerPrefs.SetString("SaveFile" + slotIndex, newName);
        PlayerPrefs.SetString("SaveFileName", newName);
        fileName = newName;
        text.text = fileName;
    }

    public void DeleteFile()
    {
        string path = Path.Combine(Application.persistentDataPath, fileName + SaveSystem.fileExtention);
        if (File.Exists(path))
        {
            File.Delete(path);
            PlayerPrefs.SetString("SaveFile" + slotIndex, "");
            PlayerPrefs.SetString("SaveFileName", "");
            FileDoesntExist();
        }
        else
        {
            Debug.LogError("File does not exist at " + path);
        }
    }

    public void Play()
    {
        PlayerPrefs.SetString("SaveFileName", fileName);
        SaveSystem.Load();
    }
}
