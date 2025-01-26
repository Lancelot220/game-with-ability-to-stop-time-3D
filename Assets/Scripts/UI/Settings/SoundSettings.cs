using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Localization.SmartFormat.Utilities;
using UnityEngine.UI;

public class SoundSettings : MonoBehaviour
{
    public AudioMixer mixer;
    public Slider master;
    public Slider sfx;
    public Slider music;

    public void SetSavedSettings()
    {
        master.value = PlayerPrefs.GetFloat("masterVolume", 1);
        sfx.value = PlayerPrefs.GetFloat("sfxVolume", 1);
        music.value = PlayerPrefs.GetFloat("musicVolume", 1);

        ChangeMaster(master.value);
        ChangeSFX(sfx.value);
        ChangeMusic(music.value);

        print("Sound was set according to the saved values");
    }
    public void ChangeMaster(float level)
    {
        mixer.SetFloat("Master", Mathf.Log10(level) * 20f);
        PlayerPrefs.SetFloat("masterVolume", level);
    }

    public void ChangeSFX(float level)
    {
        mixer.SetFloat("SFX", Mathf.Log10(level) * 20f);
        PlayerPrefs.SetFloat("sfxVolume", level);
    }

    public void ChangeMusic(float level)
    {
        mixer.SetFloat("Music", Mathf.Log10(level) * 20f);
        PlayerPrefs.SetFloat("musicVolume", level);
    }
}
