using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class uiEngimonosInventory : MonoBehaviour
{
    [SerializeField] private float dragSmooth = 0.25f;
    [SerializeField] private float snapThreshold = 300f;
    [SerializeField] private float snapSpeed = 8f;
    [SerializeField] private float backToSlotSpeed = 5f;

    private Canvas canvas;
    private RectTransform[] draggableElements;
    private RectTransform currentDrag;
    private bool isDragging = false;
    private List<RectTransform> slotList = new List<RectTransform>();
    private RectTransform slotsParent;
    private Vector2 lastMousePos;

    // Optimizaciones
    private readonly Dictionary<RectTransform, Coroutine> activeSnaps = 
        new Dictionary<RectTransform, Coroutine>(16);
    private readonly HashSet<int> occupiedSlots = new HashSet<int>(16);
    private Vector3 tempPosition = Vector3.zero;
    private Vector2 tempDistance = Vector2.zero;
    private readonly int[] slotIndices = new int[32];

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null) { Debug.LogError("No se encontró un Canvas padre."); return; }

        GameObject engimonosParent = GameObject.Find("engimonosInGame");
        if (engimonosParent == null) { Debug.LogError("No se encontró el objeto 'engimonosInGame'."); return; }

        int childCount = engimonosParent.transform.childCount;
        draggableElements = new RectTransform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            draggableElements[i] = engimonosParent.transform.GetChild(i) as RectTransform;
        }

        GameObject slotsObj = GameObject.Find("engimonosLista");
        if (slotsObj == null) { Debug.LogError("No se encontró el objeto 'engimonosLista'."); return; }

        slotsParent = slotsObj.GetComponent<RectTransform>();
        foreach (Transform child in slotsParent)
        {
            if (child.name.StartsWith("engimonoSlot"))
            {
                slotList.Add(child as RectTransform);
                int index = GetSlotIndex(child.name);
                occupiedSlots.Add(index);
            }
        }
    }

    void Update()
    {
        if (canvas == null) return;
        var mouse = Mouse.current;
        if (mouse == null) return;
        Vector2 mousePos = mouse.position.ReadValue();

        // Iniciar arrastre
        if (mouse.leftButton.wasPressedThisFrame)
        {
            foreach (var rect in draggableElements)
            {
                if (rect == canvas.GetComponent<RectTransform>())
                    continue;
                
                if (RectTransformUtility.RectangleContainsScreenPoint(
                    rect, mousePos, canvas.worldCamera))
                {
                    currentDrag = rect;
                    isDragging = true;
                    lastMousePos = mousePos;
                    currentDrag.SetAsLastSibling();
                    break;
                }
            }
        }

        // Arrastre activo
        if (isDragging && currentDrag != null && mouse.leftButton.isPressed)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                mousePos,
                canvas.worldCamera,
                out Vector2 localPoint))
            {
                currentDrag.anchoredPosition = Vector2.Lerp(
                    currentDrag.anchoredPosition,
                    localPoint,
                    dragSmooth);
            }
        }

        // Soltar
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            if (currentDrag != null)
                SnapToClosestSlotOrSwap(currentDrag);
            currentDrag = null;
            isDragging = false;
        }
    }

    void SnapToClosestSlotOrSwap(RectTransform rect)
    {
        if (activeSnaps.ContainsKey(rect))
            StopCoroutine(activeSnaps[rect]);

        RectTransform closestSlot = FindClosestSlot(rect);
        float distance = closestSlot != null 
            ? Vector2.Distance(rect.position, closestSlot.position) 
            : float.MaxValue;

        if (closestSlot != null && distance < snapThreshold)
        {
            // Verificar si el slot está ocupado
            if (occupiedSlots.Contains(GetSlotIndex(closestSlot.name)))
            {
                // Encuentra el engimono que está en el slot
                RectTransform otherEngi = null;
                foreach (var element in draggableElements)
                {
                    if (element != rect && 
                        Vector2.Distance(element.position, closestSlot.position) < 0.1f)
                    {
                        otherEngi = element;
                        break;
                    }
                }

                if (otherEngi != null)
                {
                    // Intercambiar posiciones
                    StartCoroutine(SwapPositions(rect, otherEngi, closestSlot, 
                        GetSlotIndex(closestSlot.name)));
                    return;
                }
            }

            // Si el slot está vacío o no hay intercambio
            StartCoroutine(SmoothSnap(rect, closestSlot, snapSpeed));
        }
        else
        {
            // Volver al slot original si no hay slot cercano
            if (activeSnaps.ContainsKey(rect))
                activeSnaps[rect] = StartCoroutine(SmoothSnap(
                    rect, slotList[0], backToSlotSpeed));
        }
    }

    IEnumerator SwapPositions(RectTransform rect1, RectTransform rect2, 
        RectTransform targetSlot, int targetIndex)
    {
        Vector3 pos1 = rect1.position;
        Vector3 pos2 = rect2.position;
        float elapsedTime = 0f;

        while (elapsedTime < 1f / snapSpeed)
        {
            float t = elapsedTime * snapSpeed;
            
            // Mover rect1 hacia la posición de rect2
            rect1.position = Vector3.Lerp(pos1, pos2, t);
            // Mover rect2 hacia la posición de targetSlot
            rect2.position = Vector3.Lerp(pos2, targetSlot.position, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Asegurar que lleguen exactamente a sus posiciones finales
        rect1.position = pos2;
        rect2.position = targetSlot.position;

        // Actualizar el slot ocupado
        occupiedSlots.Remove(targetIndex);
        occupiedSlots.Add(targetIndex);
    }

    IEnumerator SmoothSnap(RectTransform rect, RectTransform target, float speed)
    {
        Vector3 start = rect.position;
        Vector3 end = target.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            rect.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        rect.position = end;
    }

    RectTransform FindClosestSlot(RectTransform rect)
    {
        int slotCount = slotList.Count;
        int closestIndex = -1;
        float minDistance = float.MaxValue;

        for (int i = 0; i < slotCount; i++)
        {
            var slot = slotList[i];
            tempDistance.x = rect.position.x - slot.position.x;
            tempDistance.y = rect.position.y - slot.position.y;
            float distance = tempDistance.magnitude;

            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex != -1 ? slotList[closestIndex] : null;
    }

    int GetSlotIndex(string slotName)
    {
        if (string.IsNullOrEmpty(slotName)) return -1;
        int indexStart = slotName.LastIndexOf('#');
        if (indexStart == -1) return -1;
        
        if (int.TryParse(slotName.Substring(indexStart + 1), out int index))
            return index;
        return -1;
    }
}