using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public float rotationSpeed = 100;
    [SerializeField] protected AudioClip pickUpSound;
    [SerializeField] protected GameObject prefab;
    void RotateItem() {transform.RotateAround(transform.position, Vector3.up, rotationSpeed * Time.deltaTime);}

    void Update() { RotateItem(); }

    protected virtual void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            ApplyEffect(col.gameObject);
            Destroy(gameObject);

            GameObject sound = GameObject.Instantiate(prefab, transform.position, Quaternion.identity);
            AudioSource audioSource = sound.GetComponent<AudioSource>();
            audioSource.clip = pickUpSound;
            audioSource.Play();
            Destroy(sound, pickUpSound.length);
        }
    }

    protected virtual void ApplyEffect(GameObject player)
    {}
}
