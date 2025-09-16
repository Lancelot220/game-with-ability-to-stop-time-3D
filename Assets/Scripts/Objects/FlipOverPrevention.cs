using UnityEngine;

public class FlipOverPrevention : MonoBehaviour
{
    public float minAngle = -45f;
    public float maxAngle = 45f;
    public Vector3 axis = Vector3.up; // Local axis to restrict (e.g., Vector3.right for X)
    public Vector3 currentEulerAngles;

    void Update()
    {
        currentEulerAngles = transform.localEulerAngles;
    }
    void LateUpdate()
    {
        // Get local rotation in Euler angles
        Vector3 localEuler = transform.localEulerAngles;

        // Convert axis to string for easier handling
        axis = axis.normalized;
        if (axis == Vector3.right)
        {
            localEuler.x = ClampAngle(localEuler.x, minAngle, maxAngle);
        }
        else if (axis == Vector3.up)
        {
            localEuler.y = ClampAngle(localEuler.y, minAngle, maxAngle);
        }
        else if (axis == Vector3.forward)
        {
            localEuler.z = ClampAngle(localEuler.z, minAngle, maxAngle);
        }
        // Apply clamped rotation
        transform.localEulerAngles = localEuler;
    }

    float ClampAngle(float angle, float min, float max)
    {
        angle = NormalizeAngle(angle);
        min = NormalizeAngle(min);
        max = NormalizeAngle(max);

        // Handle wrap-around
        if (min < max)
            return Mathf.Clamp(angle, min, max);
        else
            return (angle > min || angle < max) ? angle : (Mathf.Abs(angle - min) < Mathf.Abs(angle - max) ? min : max);
    }

    float NormalizeAngle(float angle)
    {
        angle = angle % 360f;
        if (angle < 0) angle += 360f;
        return angle;
    }
}
