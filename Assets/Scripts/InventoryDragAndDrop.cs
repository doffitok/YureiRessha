using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class InventoryDragAndDrop : MonoBehaviour
{
    private Canvas canvas;
    private Camera cam;

    private InventoryItemUI dragged;
    private Vector2 offset;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        cam = canvas.worldCamera;
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryBeginDrag();
        }

        if (dragged != null && Mouse.current.leftButton.isPressed)
        {
            Drag();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && dragged != null)
        {
            Drop();
        }
    }

    private void TryBeginDrag()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        var hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
        foreach (var hit in hits)
        {
            var item = hit.collider?.GetComponent<InventoryItemUI>();
            if (item != null)
            {
                dragged = item;
                dragged.transform.SetAsLastSibling();

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    mousePos,
                    cam,
                    out offset
                );
                offset -= dragged.GetComponent<RectTransform>().anchoredPosition;

                return;
            }
        }
    }

    private void Drag()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            mousePos,
            cam,
            out var pos
        );
        dragged.GetComponent<RectTransform>().anchoredPosition = pos - offset;
    }

    private void Drop()
    {
        InventorySlotUI target = FindClosestSlot(dragged);

        if (target != null)
        {
            if (target.currentItem == null)
            {
                // normal drop
                MoveToSlot(dragged, target);
            }
            else
            {
                // intercambio
                SwapItems(dragged, target.currentItem);
            }
        }
        else
        {
            // vuelve a su slot actual
            SnapBack(dragged);
        }

        dragged = null;
    }

    private InventorySlotUI FindClosestSlot(InventoryItemUI item)
    {
        float best = float.MaxValue;
        InventorySlotUI bestSlot = null;

        foreach (var slot in FindObjectsOfType<InventorySlotUI>())
        {
            float dist = Vector2.Distance(item.transform.position, slot.transform.position);
            if (dist < best)
            {
                best = dist;
                bestSlot = slot;
            }
        }

        return bestSlot;
    }

    private void MoveToSlot(InventoryItemUI item, InventorySlotUI slot)
    {
        if (item.currentSlot != null)
            item.currentSlot.currentItem = null;

        slot.currentItem = item;
        item.currentSlot = slot;

        StartCoroutine(Snap(item.GetComponent<RectTransform>(), slot.GetComponent<RectTransform>()));
    }

    private void SwapItems(InventoryItemUI a, InventoryItemUI b)
    {
        InventorySlotUI slotA = a.currentSlot;
        InventorySlotUI slotB = b.currentSlot;

        slotA.currentItem = b;
        b.currentSlot = slotA;

        slotB.currentItem = a;
        a.currentSlot = slotB;

        StartCoroutine(Snap(a.GetComponent<RectTransform>(), slotB.GetComponent<RectTransform>()));
        StartCoroutine(Snap(b.GetComponent<RectTransform>(), slotA.GetComponent<RectTransform>()));
    }

    private void SnapBack(InventoryItemUI item)
    {
        StartCoroutine(Snap(item.GetComponent<RectTransform>(), item.currentSlot.transform as RectTransform));
    }

    private IEnumerator Snap(RectTransform rect, RectTransform target)
    {
        Vector2 start = rect.anchoredPosition;
        Vector2 end = target.localPosition;
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * 8f;
            rect.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }

        rect.anchoredPosition = end;
    }
}