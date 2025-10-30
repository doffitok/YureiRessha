using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class uiEngimonosInventoryManager : MonoBehaviour
{
    public static uiEngimonosInventoryManager Instance { get; private set; }

    [Header("Referencias")]
    [SerializeField] private Transform slotsParent;            // padre de slots (engimonosLista)
    [SerializeField] private GameObject inventoryItemPrefab;   // prefab GENÉRICO del inventario

    [Header("Opciones")]
    [SerializeField] private bool cleanChildrenOnSpawn = true; // borra hijos del prefab al instanciar

    private readonly List<InventorySlotUI> slots = new List<InventorySlotUI>();
    private GraphicRaycaster[] raycasters;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (slotsParent == null) Debug.LogError("[InventoryManager] slotsParent no asignado.");
        if (inventoryItemPrefab == null) Debug.LogError("[InventoryManager] inventoryItemPrefab no asignado.");

        slots.Clear();
        if (slotsParent != null)
        {
            foreach (Transform child in slotsParent)
            {
                var slot = child.GetComponent<InventorySlotUI>();
                if (slot != null) slots.Add(slot);
            }
        }

        raycasters = FindObjectsOfType<GraphicRaycaster>(true);
        if (raycasters == null || raycasters.Length == 0)
            Debug.LogWarning("[InventoryManager] No hay GraphicRaycaster en escena.");
    }

    /// <summary>
    /// Crea un ítem en el primer slot libre, respetando tamaño local del prefab de inventario.
    /// </summary>
    public void AddEngimono(EngimonoInstance inst)
    {
        if (inventoryItemPrefab == null) 
        { 
            Debug.LogError("inventoryItemPrefab no asignado."); 
            return; 
        }

        var free = GetFirstFreeSlot();
        if (free == null)
        {
            Debug.Log("Inventario lleno.");
            return;
        }

        var go = Instantiate(inventoryItemPrefab, free.transform);
        var itemUI = go.GetComponent<InventoryItemUI>();
        if (itemUI == null)
        {
            Debug.LogError("InventoryItemPrefab no tiene InventoryItemUI.");
            Destroy(go);
            return;
        }

        itemUI.Setup(inst, free, cleanChildrenOnSpawn);
    }

    public InventorySlotUI GetFirstFreeSlot()
    {
        foreach (var s in slots)
            if (s.IsFree()) 
                return s;

        return null;
    }

    /// <summary>
    /// Slot bajo el puntero (raycast UI)
    /// </summary>
    public InventorySlotUI GetSlotUnderPointer(PointerEventData evt)
    {
        if (raycasters == null) return null;
        var results = new List<RaycastResult>();

        foreach (var gr in raycasters)
        {
            if (gr == null || !gr.isActiveAndEnabled) continue;

            results.Clear();
            gr.Raycast(evt, results);

            foreach (var r in results)
            {
                var slot = r.gameObject.GetComponentInParent<InventorySlotUI>();
                if (slot != null) return slot;
            }
        }
        return null;
    }
}