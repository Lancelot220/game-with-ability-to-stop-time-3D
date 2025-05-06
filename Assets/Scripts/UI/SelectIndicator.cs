using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SelectIndicator : MonoBehaviour
{
    public RectTransform indicator; // Трикутничок
    public Vector3 offset = new Vector3(-20f, 0f, 0f); // Відступ від кнопки
    public float moveSpeed = 10f;
    public Selectable defaultSelectable; // Що вибирати, якщо нічого не вибрано

    private Image indicatorImage;
    bool usingMouse = true;

    InputAction navigate;
    InputAction click;
    Controls ctrls;
    void Awake() { ctrls = new Controls(); }
    void OnEnable() { navigate = ctrls.UI.Navigate; navigate.Enable(); click = ctrls.UI.Submit; click.Enable(); click.performed += OnClick;}
    void OnDisable() { navigate.Disable(); }

    void Start()
    {
        indicator = GetComponent<RectTransform>();
        if (indicator != null)
            indicatorImage = indicator.GetComponent<Image>();
    }

    void Update()
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if(navigate.ReadValue<Vector2>() != Vector2.zero) usingMouse = false;
        if (indicatorImage != null && indicatorImage.enabled && usingMouse)
                indicatorImage.enabled = false;

        if (selected != null && selected.activeInHierarchy && selected.GetComponent<Selectable>())
        {
            if (indicatorImage != null && !indicatorImage.enabled && !usingMouse)
                    indicatorImage.enabled = true;

            // Отримуємо ліву точку кнопки
            RectTransform selectedRectTransform = selected.GetComponent<RectTransform>();
            Vector3 targetPos = new Vector2(
            selectedRectTransform.position.x - (selectedRectTransform.rect.width * selectedRectTransform.pivot.x),
            selectedRectTransform.position.y + selectedRectTransform.rect.height * (0.5f - selectedRectTransform.pivot.y)
            );

            // Плавне переміщення
            targetPos += offset; // Додаємо відступ
            indicator.position = Vector3.Lerp(indicator.position, targetPos, Time.unscaledDeltaTime * moveSpeed);
        }
        else
        {
            // Автоселект при навігації
            if (navigate.ReadValue<Vector2>() != Vector2.zero)
            {
                if (defaultSelectable != null)
                {
                    EventSystem.current.SetSelectedGameObject(defaultSelectable.gameObject);
                }   
            }

            // Сховати індикатор
            if (indicatorImage != null && indicatorImage.enabled)
                indicatorImage.enabled = false;
        }
    }

    void OnClick(InputAction.CallbackContext context)
    { usingMouse = true; }
}
