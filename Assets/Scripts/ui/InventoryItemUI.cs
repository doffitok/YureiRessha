using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Estado")]
    public EngimonoInstance instance;
    public InventorySlotUI currentSlot;

    [Header("Opcional")]
    [SerializeField] private CanvasGroup canvasGroup;

    private RectTransform rect;
    private Canvas canvas;

    // ADN del prefab (para que nunca cambie)
    private Vector2 prefabSizeDelta;
    private Vector3 prefabLocalScale;
    private Vector2 prefabPivot;
    private Vector2 prefabAnchorsMin;
    private Vector2 prefabAnchorsMax;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        prefabSizeDelta   = rect.sizeDelta;
        prefabLocalScale  = rect.localScale;
        prefabPivot       = rect.pivot;
        prefabAnchorsMin  = rect.anchorMin;
        prefabAnchorsMax  = rect.anchorMax;
    }

    public void Setup(EngimonoInstance inst, InventorySlotUI slot, bool cleanChildren = false)
    {
        instance    = inst;
        currentSlot = slot;

        var img = GetComponent<Image>();
        img.sprite = inst.data != null ? inst.data.Icono : null;
        img.raycastTarget = true;

        if (cleanChildren)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
        }

        transform.SetParent(slot.transform, worldPositionStays: false);

        rect.anchorMin = prefabAnchorsMin;
        rect.anchorMax = prefabAnchorsMax;
        rect.pivot     = prefabPivot;

        rect.sizeDelta  = prefabSizeDelta;
        rect.localScale = prefabLocalScale;

        rect.anchoredPosition = Vector2.zero;

        Vector3 parentLossy = slot.transform.lossyScale;
        rect.localScale = new Vector3(
            prefabLocalScale.x / (Mathf.Approximately(parentLossy.x,0f)?1f:parentLossy.x),
            prefabLocalScale.y / (Mathf.Approximately(parentLossy.y,0f)?1f:parentLossy.y),
            prefabLocalScale.z / (Mathf.Approximately(parentLossy.z,0f)?1f:parentLossy.z)
        );

        slot.currentItem = this;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (instance == null || !instance.comprado) return;

        transform.SetParent(canvas.transform, worldPositionStays: true);

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.92f;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (instance == null || !instance.comprado) return;

        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out pos
        );
        rect.anchoredPosition = pos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (instance == null || !instance.comprado) return;

        InventorySlotUI target = uiEngimonosInventoryManager.Instance.GetSlotUnderPointer(eventData);

        if (target == null)
        {
            AttachToSlot(currentSlot);
        }
        else if (target.IsFree()) // ← CORREGIDO AQUÍ
        {
            MoveToSlot(target);
        }
        else
        {
            SwapWith(target.currentItem);
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }
    }

    private void AttachToSlot(InventorySlotUI slot)
    {
        transform.SetParent(slot.transform, worldPositionStays: false);

        rect.anchorMin = prefabAnchorsMin;
        rect.anchorMax = prefabAnchorsMax;
        rect.pivot     = prefabPivot;
        rect.sizeDelta = prefabSizeDelta;
        rect.anchoredPosition = Vector2.zero;

        Vector3 parentLossy = slot.transform.lossyScale;
        rect.localScale = new Vector3(
            prefabLocalScale.x / (Mathf.Approximately(parentLossy.x,0f)?1f:parentLossy.x),
            prefabLocalScale.y / (Mathf.Approximately(parentLossy.y,0f)?1f:parentLossy.y),
            prefabLocalScale.z / (Mathf.Approximately(parentLossy.z,0f)?1f:parentLossy.z)
        );
    }

    private void MoveToSlot(InventorySlotUI newSlot)
    {
        if (currentSlot != null) currentSlot.currentItem = null;
        currentSlot = newSlot;
        currentSlot.currentItem = this;
        AttachToSlot(newSlot);
    }

    private void SwapWith(InventoryItemUI other)
    {
        var slotA = currentSlot;
        var slotB = other.currentSlot;

        slotA.currentItem = other;
        other.currentSlot = slotA;
        other.AttachToSlot(slotA);

        slotB.currentItem = this;
        currentSlot = slotB;
        AttachToSlot(slotB);
    }
}