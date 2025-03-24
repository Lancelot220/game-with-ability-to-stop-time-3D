using UnityEngine;

public class SwingingObject : MonoBehaviour
{
    public float minAngle = -30f; // Мінімальний кут
    public float maxAngle = 30f;  // Максимальний кут
    public Vector3 rotationAxis = Vector3.forward; // Вісь обертання
    public float speed = 1f; // Швидкість руху

    [SerializeField] private float currentAngleDisplay; // Поточний кут (для перегляду в інспекторі)

    private void Update()
    {
        float t = Mathf.PingPong(Time.time * speed, 1); // Коливається від 0 до 1 і назад
        float currentAngle = Mathf.Lerp(minAngle, maxAngle, t); // Інтерполяція між кутами

        transform.localRotation = Quaternion.AngleAxis(currentAngle, rotationAxis);

        // Оновлення значення для перегляду в інспекторі
        currentAngleDisplay = currentAngle;
    }
}
