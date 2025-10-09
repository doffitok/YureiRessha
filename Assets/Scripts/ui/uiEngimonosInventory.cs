using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class uiEngimonosInventory : MonoBehaviour
{
    private Canvas canvas;
    private RectTransform[] draggableElements;
    private RectTransform currentDrag;
    private bool isDragging = false;

    private List<RectTransform> slotList = new List<RectTransform>();
    private RectTransform slotsParent;

    private Vector2 lastMousePos;
    private Vector2 velocity;

    [Header("Configuración visual")]
    [SerializeField] private float dragSmooth = 0.25f;       // suavizado mientras arrastras
    [SerializeField] private float snapThreshold = 300f;     // distancia máxima para hacer snap
    [SerializeField] private float snapSpeed = 8f;           // velocidad del snap a un slot cercano
    [SerializeField] private float backToSlotSpeed = 5f;     // velocidad de regreso al último slot si no hay slot cercano

    // Guardar el último slot de cada engimono
    private Dictionary<RectTransform, RectTransform> lastSlotMap = new Dictionary<RectTransform, RectTransform>();

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No se encontró un Canvas padre.");
            return;
        }

        GameObject engimonosParent = GameObject.Find("engimonosInGame");
        if (engimonosParent == null)
        {
            Debug.LogError("No se encontró el objeto 'engimonosInGame'.");
            return;
        }
        draggableElements = engimonosParent.GetComponentsInChildren<RectTransform>(true);

        GameObject slotsObj = GameObject.Find("engimonosLista");
        if (slotsObj == null)
        {
            Debug.LogError("No se encontró el objeto 'engimonosLista'.");
            return;
        }

        slotsParent = slotsObj.GetComponent<RectTransform>();
        foreach (Transform child in slotsParent)
        {
            if (child.name.StartsWith("engimonoSlot"))
                slotList.Add(child as RectTransform);
        }

        if (slotList.Count == 0)
            Debug.LogWarning("No se encontraron slots con nombre 'engimonoSlot##'.");

        // Inicializar diccionario con el slot más cercano inicial para cada engimono
        foreach (var engi in draggableElements)
        {
            RectTransform closest = FindClosestSlot(engi);
            if (closest != null)
                lastSlotMap[engi] = closest;
        }
    }

    void Update()
    {
        if (canvas == null) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 mousePos = mouse.position.ReadValue();

        // --- Iniciar arrastre ---
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
                    currentDrag.SetAsLastSibling(); // poner al frente
                    break;
                }
            }
        }

        // --- Arrastre activo ---
        if (isDragging && currentDrag != null && mouse.leftButton.isPressed)
        {
            Vector2 pos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                mousePos,
                canvas.worldCamera,
                out pos))
            {
                currentDrag.anchoredPosition = Vector2.Lerp(
                    currentDrag.anchoredPosition, pos, dragSmooth);
            }
        }

        // --- Soltar ---
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            if (currentDrag != null)
                SnapToClosestSlotOrReturn(currentDrag);

            currentDrag = null;
            isDragging = false;
        }
    }

    void SnapToClosestSlotOrReturn(RectTransform rect)
    {
        RectTransform closestSlot = FindClosestSlot(rect);

        if (closestSlot != null && Vector2.Distance(rect.position, closestSlot.position) < snapThreshold)
        {
            lastSlotMap[rect] = closestSlot; // actualizar último slot
            StartCoroutine(SmoothSnap(rect, closestSlot, snapSpeed));
        }
        else
        {
            // No hay slot cercano: volver al último slot registrado con velocidad backToSlotSpeed
            if (lastSlotMap.ContainsKey(rect) && lastSlotMap[rect] != null)
                StartCoroutine(SmoothSnap(rect, lastSlotMap[rect], backToSlotSpeed));
        }
    }

    RectTransform FindClosestSlot(RectTransform rect)
    {
        RectTransform closestSlot = null;
        float closestDist = float.MaxValue;

        foreach (var slot in slotList)
        {
            float dist = Vector2.Distance(rect.position, slot.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestSlot = slot;
            }
        }

        return closestSlot;
    }

    System.Collections.IEnumerator SmoothSnap(RectTransform rect, RectTransform target, float speed)
    {
        Vector2 start = rect.anchoredPosition;
        Vector2 end = rect.parent.InverseTransformPoint(target.position);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            rect.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }

        rect.anchoredPosition = end;
    }
}