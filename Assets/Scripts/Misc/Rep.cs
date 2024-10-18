using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Rep : MonoBehaviour
{
    static public void PlayOnce(AudioClip clip, GameObject prefab, GameObject gameObject)
    {
        GameObject sound = GameObject.Instantiate(prefab, gameObject.transform.position, Quaternion.identity);
        AudioSource audioSource = sound.GetComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.Play();
        Destroy(sound, clip.length);
    }
}
