using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class StopTimeCDBoost : MonoBehaviour
{
    public int cdBoost = 5;
    public float rotationSpeed = 100;

    void Update()
    {
        transform.RotateAround(transform.position, Vector3.up, rotationSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.CompareTag("Player"))
        {
            StopTime_ st = col.gameObject.GetComponent<StopTime_>();
            st.cdTimer += cdBoost;
            if(st.cdTimer > st.cd) st.cdTimer = st.cd;
            print($"Now you have to wait only {st.cd - st.cdTimer} second(s)!");
            Destroy(gameObject);
        }
    }
}
