using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
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

    [Header("Configuración visual")]
    [SerializeField] private float dragSmooth = 0.25f;
    [SerializeField] private float snapThreshold = 300f;
    [SerializeField] private float snapSpeed = 8f;
    [SerializeField] private float backToSlotSpeed = 5f;

    private Dictionary<RectTransform, RectTransform> lastSlotMap = new Dictionary<RectTransform, RectTransform>();
    private RectTransform highlightedSlot;

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
            {
                var rect = child as RectTransform;
                slotList.Add(rect);

                // Aseguramos que cada slot tenga el script uiInventorySlotState
                if (rect.GetComponent<uiInventorySlotState>() == null)
                    rect.gameObject.AddComponent<uiInventorySlotState>();
            }
        }

        // Inicializar colores
        foreach (var slot in slotList)
            slot.GetComponent<uiInventorySlotState>().SetState(false);

        // Inicializar diccionario con el slot más cercano inicial para cada engimono
        foreach (var engi in draggableElements)
        {
            RectTransform closest = FindClosestSlot(engi);
            if (closest != null)
            {
                lastSlotMap[engi] = closest;
                closest.GetComponent<uiInventorySlotState>().SetState(true);
            }
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
                    currentDrag.SetAsLastSibling();
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
                currentDrag.anchoredPosition = Vector2.Lerp(currentDrag.anchoredPosition, pos, dragSmooth);
            }

            HighlightClosestSlot(currentDrag);
        }

        // --- Soltar ---
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            if (currentDrag != null)
                SnapToClosestSlotOrReturn(currentDrag);

            ClearHighlight();
            currentDrag = null;
            isDragging = false;
        }
    }

    void HighlightClosestSlot(RectTransform rect)
    {
        RectTransform closest = FindClosestSlot(rect);
        if (closest == null) return;

        if (highlightedSlot != closest)
        {
            ClearHighlight();
            highlightedSlot = closest;

            var slotState = closest.GetComponent<uiInventorySlotState>();
            if (slotState != null && !slotState.isOccupied)
                slotState.SetColor(Color.green);
        }
    }

    void ClearHighlight()
    {
        if (highlightedSlot != null)
        {
            var slotState = highlightedSlot.GetComponent<uiInventorySlotState>();
            if (slotState != null)
            {
                if (slotState.isOccupied)
                    slotState.SetColor(Color.red);
                else
                    slotState.SetColor(Color.white);
            }
            highlightedSlot = null;
        }
    }

    void SnapToClosestSlotOrReturn(RectTransform rect)
    {
        RectTransform closestSlot = FindClosestSlot(rect);
        RectTransform lastSlot = lastSlotMap.ContainsKey(rect) ? lastSlotMap[rect] : null;

        if (closestSlot != null && Vector2.Distance(rect.position, closestSlot.position) < snapThreshold)
        {
            var slotState = closestSlot.GetComponent<uiInventorySlotState>();
            if (!slotState.isOccupied)
            {
                // Liberar el slot anterior
                if (lastSlot != null && lastSlot != closestSlot)
                    lastSlot.GetComponent<uiInventorySlotState>().SetState(false);

                // Ocupar el nuevo slot
                slotState.SetState(true);
                lastSlotMap[rect] = closestSlot;

                StartCoroutine(SmoothSnap(rect, closestSlot, snapSpeed));
                return;
            }
        }

        // No hay slot válido: volver al último
        if (lastSlot != null)
            StartCoroutine(SmoothSnap(rect, lastSlot, backToSlotSpeed));
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