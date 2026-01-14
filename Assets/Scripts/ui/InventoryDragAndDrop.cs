using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class InventoryDragAndDrop : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform slotsParent;
    [SerializeField] private RectTransform dragLayer;

    [Header("Depuración")]
    [SerializeField] private bool debugLogs = true;

    private Canvas canvas;
    private Camera cam;
    private EngimonoItem dragged;
    private RectTransform draggedRect;
    private Transform originalParent;
    private int originalIndex;
    private CanvasGroup draggedCG;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        cam = canvas != null ? canvas.worldCamera : null;

        if (slotsParent == null)
        {
            var inv = FindFirstObjectByType<EngimonosInventoryManager>();
            if (inv != null)
                slotsParent = inv.transform.childCount > 0 ? inv.transform.GetChild(0) : inv.transform;
        }

        if (dragLayer == null && canvas != null)
        {
            var go = new GameObject("DragLayer", typeof(RectTransform));
            dragLayer = go.GetComponent<RectTransform>();
            dragLayer.SetParent(canvas.transform, false);
            dragLayer.SetAsLastSibling();
        }
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
            TryBeginDrag();

        if (dragged != null && Mouse.current.leftButton.isPressed)
            Drag();

        if (Mouse.current.leftButton.wasReleasedThisFrame && dragged != null)
            Drop();
    }

    // === INICIAR DRAG ===
    private void TryBeginDrag()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        var pointer = new PointerEventData(EventSystem.current) { position = mousePos };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);

        foreach (var r in results)
        {
            var item = r.gameObject.GetComponent<EngimonoItem>();
            if (item != null && item.Comprado)
            {
                dragged = item;
                draggedRect = item.transform as RectTransform;

                originalParent = item.transform.parent;
                originalIndex = item.transform.GetSiblingIndex();

                draggedCG = item.GetComponent<CanvasGroup>();
                if (draggedCG == null)
                    draggedCG = item.gameObject.AddComponent<CanvasGroup>();
                draggedCG.blocksRaycasts = false;
                draggedCG.alpha = 0.9f;

                if (dragLayer != null)
                    draggedRect.SetParent(dragLayer, true);

                if (debugLogs)
                    Debug.Log($"[DragDrop] 🖐 Arrastrando '{item.Nombre}' desde {originalParent.name}");
                return;
            }
        }
    }

    // === DRAG ===
    private void Drag()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, mousePos, cam, out var localPoint);
        draggedRect.anchoredPosition = localPoint;
    }

    // === DROP ===
    private void Drop()
    {
        if (dragged == null) return;

        Transform targetSlot = DetectarSlotBajoMouse();
        if (targetSlot == null)
        {
            draggedRect.SetParent(originalParent, false);
            draggedRect.SetSiblingIndex(originalIndex);
            ResetVisual();
            if (debugLogs) Debug.Log($"[DragDrop] ↩ Volviendo a {originalParent.name} (sin slot válido)");
            dragged = null;
            return;
        }

        var other = BuscarHijoEngimono(targetSlot);

        if (other != null && other != dragged)
        {
            // Intercambio
            var parentA = originalParent;
            var parentB = other.transform.parent;

            other.transform.SetParent(parentA, false);
            dragged.transform.SetParent(parentB, false);

            if (debugLogs)
                Debug.Log($"[DragDrop] 🔄 Intercambio '{dragged.Nombre}' ↔ '{other.Nombre}'");
        }
        else
        {
            // Slot vacío → mover
            dragged.transform.SetParent(targetSlot, false);
            if (debugLogs)
                Debug.Log($"[DragDrop] ✅ Movido '{dragged.Nombre}' a slot vacío '{targetSlot.name}'");
        }

        ResetVisual();
        dragged = null;
    }

    // === DETECTAR SLOT BAJO EL CURSOR ===
    private Transform DetectarSlotBajoMouse()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Transform mejorSlot = null;
        float mejorDist = float.MaxValue;

        foreach (Transform slot in slotsParent)
        {
            var rect = slot as RectTransform;
            if (rect == null) continue;

            bool dentro = RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos, cam);

            if (dentro)
            {
                if (debugLogs) Debug.Log($"[DragDrop] 🎯 Slot bajo mouse: {slot.name}");
                return slot;
            }

            // Si no está dentro, usar la distancia al centro como fallback
            float dist = Vector2.Distance(mousePos, RectTransformUtility.WorldToScreenPoint(cam, rect.position));
            if (dist < mejorDist)
            {
                mejorDist = dist;
                mejorSlot = slot;
            }
        }

        if (debugLogs && mejorSlot != null)
            Debug.Log($"[DragDrop] 🔍 Fallback slot más cercano: {mejorSlot.name} (dist={mejorDist:0.0})");

        return mejorSlot;
    }

    // === RESET VISUAL ===
    private void ResetVisual()
    {
        if (draggedCG != null)
        {
            draggedCG.blocksRaycasts = true;
            draggedCG.alpha = 1f;
        }

        if (draggedRect != null)
        {
            draggedRect.anchoredPosition = Vector2.zero;
            draggedRect.localScale = Vector3.one;
            draggedRect.localRotation = Quaternion.identity;
        }

        draggedCG = null;
        draggedRect = null;
    }

    // === BUSCAR ENGIMONO EN SLOT ===
    private EngimonoItem BuscarHijoEngimono(Transform slot)
    {
        foreach (Transform child in slot)
        {
            var eng = child.GetComponent<EngimonoItem>();
            if (eng != null)
                return eng;
        }
        return null;
    }
}