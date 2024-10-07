using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    AudioClip[] sounds;
    //[SerializeField] AudioSource audioSource;
    [SerializeField] GameObject prefab;
    public enum Surface { Grass, Metal }
    public Surface currsentSurface;
    [Header("Walk Sounds")]
    public AudioClip[] grass;
    public AudioClip[] metal;
    void Start() {currsentSurface = Surface.Metal; sounds = metal; }

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
            

            GameObject stepSound = GameObject.Instantiate(prefab, transform.position, Quaternion.identity);
            AudioSource audioSource = stepSound.GetComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.Play();
            Destroy(stepSound, audioClip.length);
        }
    }
}
