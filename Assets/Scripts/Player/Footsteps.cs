using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    AudioClip[] sounds;
    [SerializeField] AudioSource audioSource;

    public enum Surface { Grass, Metal }
    public Surface currsentSurface;
    [Header("Walk Sounds")]
    public AudioClip[] grass;
    public AudioClip[] metal;
    void Start() {currsentSurface = Surface.Metal; }

    void Update()
    {
        switch (currsentSurface)
        {
            case Surface.Metal: sounds = metal; break;
            case Surface.Grass: sounds = grass; break;
        }
    }
    public void PlaySound()
    {
        if(GetComponentInParent<Movement>().onGround)
        {
            AudioClip audioClip = sounds[Random.Range(0, sounds.Length)];
            audioSource.clip = audioClip;
            audioSource.Play();
        }
    }
}
