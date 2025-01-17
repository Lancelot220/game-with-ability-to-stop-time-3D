using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class AutoCameraRotate : MonoBehaviour
{
    public float targetAngle;
    public float rotatingSpeed = 1;
    CinemachineFreeLook camera;

    void Start()
    {
        camera = GameObject.Find("FreeLook Camera").GetComponent<CinemachineFreeLook>();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            StartCoroutine(RotateCamera());
        }
    }

    IEnumerator RotateCamera()
    {
        float startAngle = camera.m_XAxis.Value;
        float deltaAngle = Mathf.DeltaAngle(startAngle, targetAngle);
        float elapsedTime = 0;

        while (elapsedTime < rotatingSpeed)
        {
            float currentAngle = Mathf.Lerp(startAngle, startAngle + deltaAngle, elapsedTime / rotatingSpeed);
            camera.m_XAxis.Value = currentAngle;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        camera.m_XAxis.Value = targetAngle;
    }
}