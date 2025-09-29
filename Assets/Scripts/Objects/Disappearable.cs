using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disappearable : MonoBehaviour
{
    public float disappearTime = 3;
    public float maxDistance;
    Vector3 startPos;
    float timer;
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(startPos, transform.position) > maxDistance)
        {
            timer += Time.deltaTime;
            if (timer >= disappearTime)
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            timer = 0;
        }
    }
}
