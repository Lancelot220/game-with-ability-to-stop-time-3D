using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Item : MonoBehaviour
{
    public float rotationSpeed = 100;
    public bool isRotating = true;
    [SerializeField] AudioClip pickUpSound;
    [SerializeField] GameObject prefab;
    public UnityEvent<GameObject> onPickUp;
    void Update() { if(isRotating) transform.RotateAround(transform.position, Vector3.up, rotationSpeed * Time.deltaTime); }

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            if(onPickUp != null) onPickUp.Invoke(col.gameObject);
            Destroy(gameObject);

            Rep.PlayOnce(pickUpSound, prefab, gameObject);
        }
    }
}
