using UnityEngine;
using UnityEngine.InputSystem; // Usamos el nuevo Input System

public class DragUIElements_Stylized : MonoBehaviour
{
    private RectTransform[] draggableElements;
    private RectTransform currentDrag;
    private Canvas canvas;
    private bool isDragging = false;

    // Variables para efectos
    private Vector2 lastMousePos;
    private Vector2 velocity;
    private float rotationAmount = 30f; // Grados máximos de inclinación
    private float squashFactor = 0.03f;  // Cuánto se estira o aplasta
    private float smoothTime = 2f;   // Suavizado del movimiento visual

    void Start()
    {
        GameObject parent = GameObject.Find("engimonosInGame");
        if (parent == null)
        {
            Debug.LogError("No se encontró el objeto 'engimonosInGame' en la escena.");
            return;
        }

        draggableElements = parent.GetComponentsInChildren<RectTransform>(true);
        canvas = parent.GetComponentInParent<Canvas>();
        if (canvas == null)
            Debug.LogError("No se encontró un Canvas padre.");
    }

    void Update()
    {
        if (canvas == null) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 mousePos = mouse.position.ReadValue();

        // Detectar clic inicial
        if (mouse.leftButton.wasPressedThisFrame)
        {
            foreach (var rect in draggableElements)
            {
                if (rect == canvas.GetComponent<RectTransform>())
                    continue;

                if (RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos, canvas.worldCamera))
                {
                    currentDrag = rect;
                    isDragging = true;
                    lastMousePos = mousePos;
                    break;
                }
            }
        }

        // Arrastrar con estilo
        if (isDragging && currentDrag != null && mouse.leftButton.isPressed)
        {
            Vector2 pos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                mousePos,
                canvas.worldCamera,
                out pos))
            {
                // Movimiento base
                currentDrag.anchoredPosition = Vector2.Lerp(
                    currentDrag.anchoredPosition, pos, 0.3f);

                // Calcular velocidad del mouse
                velocity = (mousePos - lastMousePos) / Time.deltaTime;

                // Aplicar squash/stretch y rotación
                ApplyStylizedTransform(currentDrag, velocity);

                lastMousePos = mousePos;
            }
        }

        // Soltar
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            if (currentDrag != null)
                ResetTransform(currentDrag);

            currentDrag = null;
            isDragging = false;
        }
    }

    void ApplyStylizedTransform(RectTransform rect, Vector2 vel)
    {
        // Calcular intensidad según la magnitud de la velocidad
        float speed = vel.magnitude * 0.001f;
        speed = Mathf.Clamp(speed, 0f, 1.5f);

        // Direcciones normalizadas
        Vector2 dir = vel.normalized;

        // Stretch/Squash proporcional a la dirección real del movimiento
        float stretchX = 1f + Mathf.Abs(dir.x) * speed * squashFactor;
        float stretchY = 1f + Mathf.Abs(dir.y) * speed * squashFactor;

        // Mantener proporciones (ligeramente compensadas para no deformar de más)
        stretchX *= 1f - (Mathf.Abs(dir.y) * 0.3f);
        stretchY *= 1f - (Mathf.Abs(dir.x) * 0.3f);

        // Rotación contraria al movimiento horizontal
        float rot = Mathf.Clamp(-vel.x * 0.02f, -rotationAmount, rotationAmount);

        // Aplicar suavemente
        rect.localScale = Vector3.Lerp(rect.localScale, new Vector3(stretchX, stretchY, 1f), 0.4f);
        rect.localRotation = Quaternion.Lerp(rect.localRotation, Quaternion.Euler(0, 0, rot), 0.3f);
    }

    void ResetTransform(RectTransform rect)
    {
        // Volver suavemente al estado normal
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }
}