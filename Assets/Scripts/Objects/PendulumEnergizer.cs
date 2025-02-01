using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HingeJoint))]
public class PendulumEnergizer : MonoBehaviour
{
    [SerializeField] private float equilibriumThreshold = 1f; // Межа рівноваги (в градусах)
    [SerializeField] private float energyBoostFactor = 1.1f; // Скільки % швидкості компенсуємо (1.0 = рівно цільова, більше = додаткове підсилення)

    private bool targetValuesSet = false;
    private float targetEquilibriumSpeedDeg = 0f;
    private bool hasInjectedThisCycle = false;

    private Rigidbody rb;
    private HingeJoint hingeJoint;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        hingeJoint = GetComponent<HingeJoint>();
    }

    private void FixedUpdate()
    {
        float currentAngle = hingeJoint.angle;
        float currentAngularSpeedDeg = Vector3.Dot(rb.angularVelocity, hingeJoint.axis) * Mathf.Rad2Deg;

        if (!targetValuesSet && Mathf.Abs(currentAngle) < equilibriumThreshold)
        {
            targetEquilibriumSpeedDeg = Mathf.Abs(currentAngularSpeedDeg);
            if (targetEquilibriumSpeedDeg > 0.1f)
            {
                targetValuesSet = true;
                Debug.Log("Запам'ятано швидкість: " + targetEquilibriumSpeedDeg + " град/сек");
            }
        }

        if (targetValuesSet)
        {
            if (Mathf.Abs(currentAngle) < equilibriumThreshold)
            {
                if (!hasInjectedThisCycle)
                {
                    float deltaSpeedDeg = (targetEquilibriumSpeedDeg * energyBoostFactor) - Mathf.Abs(currentAngularSpeedDeg);
                    if (deltaSpeedDeg > 0)
                    {
                        float deltaSpeedRad = deltaSpeedDeg * Mathf.Deg2Rad;
                        float sign = Mathf.Sign(currentAngularSpeedDeg);
                        rb.AddTorque(hingeJoint.axis * deltaSpeedRad * sign, ForceMode.VelocityChange);
                        Debug.Log("Підсилено маятник на " + deltaSpeedDeg + " град/сек");
                    }
                    hasInjectedThisCycle = true;
                }
            }
            else
            {
                hasInjectedThisCycle = false;
            }
        }
    }
}
