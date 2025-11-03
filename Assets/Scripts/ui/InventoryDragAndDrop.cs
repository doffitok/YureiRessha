using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

////////////////////////////////////////////////////////////////////////////////////////////
// sistema de arrastre y suelta del inventario
//
// este script permite arrastrar y soltar items dentro del inventario
// detecta clicks del mouse y controla el inicio, arrastre y fin de la accion
// si se suelta un item sobre otro slot vacio se mueve
// si se suelta sobre un slot ocupado los intercambia
// si no hay slot cerca vuelve al slot original
// en realidad es algo solo visual, al momento de escribir esto no tiene mucha utilidad practica mas alla de que se vea lindo :P
////////////////////////////////////////////////////////////////////////////////////////////

public class InventoryDragAndDrop : MonoBehaviour
{
    ////////////////////////////////////////////////////////////////////////////////////////////
    // referencias principales
    ////////////////////////////////////////////////////////////////////////////////////////////
    private Canvas canvas;
    private Camera cam;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // variables de control del arrastre
    ////////////////////////////////////////////////////////////////////////////////////////////
    private InventoryItemUI dragged;
    private Vector2 offset;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // awake inicializa referencias
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        cam = canvas.worldCamera;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // update controla cada frame el estado del mouse
    ////////////////////////////////////////////////////////////////////////////////////////////
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

    ////////////////////////////////////////////////////////////////////////////////////////////
    // intenta iniciar el arrastre si se clickea sobre un item
    ////////////////////////////////////////////////////////////////////////////////////////////
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

    ////////////////////////////////////////////////////////////////////////////////////////////
    // mueve el item mientras se arrastra
    ////////////////////////////////////////////////////////////////////////////////////////////
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

    ////////////////////////////////////////////////////////////////////////////////////////////
    // suelta el item y determina donde debe quedar
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void Drop()
    {
        InventorySlotUI target = FindClosestSlot(dragged);

        if (target != null)
        {
            if (target.currentItem == null)
            {
                MoveToSlot(dragged, target);
            }
            else
            {
                SwapItems(dragged, target.currentItem);
            }
        }
        else
        {
            SnapBack(dragged);
        }

        dragged = null;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // busca el slot mas cercano al item soltado
    ////////////////////////////////////////////////////////////////////////////////////////////
    private InventorySlotUI FindClosestSlot(InventoryItemUI item)
    {
        float best = float.MaxValue;
        InventorySlotUI bestSlot = null;

        // esto no deberia funcionar pero lo hice funcionar asi asi que mejor no tocarlo :c
        foreach (var slot in FindObjectsByType<InventorySlotUI>(FindObjectsSortMode.None))
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

    ////////////////////////////////////////////////////////////////////////////////////////////
    // mueve un item a un slot vacio
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void MoveToSlot(InventoryItemUI item, InventorySlotUI slot)
    {
        if (item.currentSlot != null)
            item.currentSlot.currentItem = null;

        slot.currentItem = item;
        item.currentSlot = slot;

        StartCoroutine(Snap(item.GetComponent<RectTransform>(), slot.GetComponent<RectTransform>()));
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // intercambia dos items de lugar
    ////////////////////////////////////////////////////////////////////////////////////////////
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

    ////////////////////////////////////////////////////////////////////////////////////////////
    // devuelve el item a su slot original
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void SnapBack(InventoryItemUI item)
    {
        StartCoroutine(Snap(item.GetComponent<RectTransform>(), item.currentSlot.transform as RectTransform));
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // animacion suave para mover el item hacia un destino
    ////////////////////////////////////////////////////////////////////////////////////////////
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