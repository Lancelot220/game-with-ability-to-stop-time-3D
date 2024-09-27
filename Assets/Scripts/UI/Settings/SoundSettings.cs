using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundSettings : MonoBehaviour
{
    public AudioMixer mixer;
    public void ChangeMaster(float level)
    {
        mixer.SetFloat("Master", Mathf.Log10(level) * 20f);
    }

    public void ChangeSFX(float level)
    {
        mixer.SetFloat("SFX", Mathf.Log10(level) * 20f);
    }

    public void ChangeMusic(float level)
    {
        mixer.SetFloat("Music", Mathf.Log10(level) * 20f);
    }
}
