using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundsManager : MonoBehaviour
{
    [SerializeField] GameObject prefab;

    [SerializeField] AudioSource sword;
    [SerializeField] AudioClip[] attackSounds;

    public void AttackSound()
    {
        sword.clip = attackSounds[Random.Range(0, attackSounds.Length)];
        sword.Play();
    }

    
/*
    [SerializeField] AudioClip[] landSounds;

    public void LandSound()
    {
        AudioClip audioClip = landSounds[Random.Range(0, landSounds.Length)];
            
        GameObject landSound = GameObject.Instantiate(prefab, transform.position, Quaternion.identity);
        AudioSource audioSource = landSound.GetComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.Play();
    }*/
}
