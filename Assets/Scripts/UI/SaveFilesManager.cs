using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using TMPro.EditorUtilities;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.Localization.Components;
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
        if (PlayerPrefs.GetString("SaveFile" + slotIndex, "") != "" &&
        File.Exists(Path.Combine(Application.persistentDataPath, PlayerPrefs.GetString("SaveFile" + slotIndex) + SaveSystem.fileExtention)))
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
        GetComponent<LocalizeStringEvent>().enabled = false;
        fileName = PlayerPrefs.GetString("SaveFile" + slotIndex);
        text.text = fileName;
        fileMananagementButtons.SetActive(true);
        fileCreationButtons.SetActive(false);
        string lastSave = File.GetLastWriteTime(Path.Combine(Application.persistentDataPath, fileName + SaveSystem.fileExtention)).ToString("dd.MM.yyyy HH:mm:ss");
        if (lastSave == File.GetCreationTime(Path.Combine(Application.persistentDataPath, fileName + SaveSystem.fileExtention)).ToString("dd.MM.yyyy HH:mm:ss"))
            lastSaveText.text = "--";
        else
            lastSaveText.text = lastSave;
        fileMananagementButtons.GetComponentInChildren<TMP_InputField>(true).text = fileName;
    }

    void FileDoesntExist()
    {
        GetComponent<LocalizeStringEvent>().enabled = true;
        fileCreationButtons.SetActive(true);
        fileMananagementButtons.SetActive(false);
    }

    public void CreateFile(string name)
    {
        name = CleanAndValidateName(name, false);
        if (name == null) return;
        fileName = name;
        PlayerPrefs.SetString("SaveFile" + slotIndex, fileName);
        PlayerPrefs.SetString("SaveFileName", fileName);
        SaveSystem.Save(null, new List<string>(), 0, new List<string>(), null); // Initial save with default values
        FileExists();
    }

    public void EditName(string newName)
    {
        newName = CleanAndValidateName(newName, true);
        if (newName == null) return;
        File.Move(Path.Combine(Application.persistentDataPath, fileName + SaveSystem.fileExtention), Path.Combine(Application.persistentDataPath, newName + SaveSystem.fileExtention));
        PlayerPrefs.SetString("SaveFile" + slotIndex, newName);
        PlayerPrefs.SetString("SaveFileName", newName);
        fileName = newName;
        text.text = fileName;
        //fileMananagementButtons.GetComponentInChildren<TMP_InputField>(true).text = fileName;
    }

    public void DeleteFile()
    {
        string path = Path.Combine(Application.persistentDataPath, fileName + SaveSystem.fileExtention);
        if (File.Exists(path))
        {
            File.Delete(path);
            PlayerPrefs.SetString("SaveFile" + slotIndex, "");
            PlayerPrefs.SetString("SaveFileName", "");
            SaveSystem.currentSaveData = null;
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

    string CleanAndValidateName(string inputName, bool rename)
    {
        // 1. Перевірка на порожнє ім’я
        if (string.IsNullOrWhiteSpace(inputName))
        {
            if (!rename) GameObject.Find("Message:EmptyName").GetComponent<Animator>().Play("ShowUp");
            return null;
        }

        // 2. Заборонені символи → замінити на '_'
        var invalidChars = Path.GetInvalidFileNameChars();
        if (inputName.IndexOfAny(invalidChars) >= 0)
        {
            GameObject.Find("Message:InvalidName").GetComponent<Animator>().Play("ShowUp");
            inputName = new string(inputName.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
        }

        // 3. Обрізаємо до 20 символів
        // if (inputName.Length > 20)
        // {
        //     inputName = inputName.Substring(0, 20);
        //     GameObject.Find("Message:NameTooLong").GetComponent<Animator>().Play("ShowUp");
        // }

        // 4. Унікальна назва — додаємо (1), (2), ...
        string baseName = inputName;
        string extension = SaveSystem.fileExtention;
        string path = Path.Combine(Application.persistentDataPath, baseName + extension);
        int counter = 1;

        while (File.Exists(path) && inputName != fileName)
        {
            baseName = $"{inputName} ({counter})";
            if (baseName.Length > 20)
                baseName = baseName.Substring(0, 20); // захист від перевищення
            path = Path.Combine(Application.persistentDataPath, baseName + extension);
            counter++;
        }

        // Якщо була зміна назви — показуємо повідомлення
        if (baseName != inputName)
        {
            GameObject.Find("Message:FileExists").GetComponent<Animator>().Play("ShowUp");
        }

        return baseName;
    }

}
