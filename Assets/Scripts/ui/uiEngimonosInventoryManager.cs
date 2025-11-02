using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

////////////////////////////////////////////////////////////////////////////////////////////
// administrador del inventario de engimonos
//
// este script administra todos los slots del inventario y controla donde se crean los engimonos
// mantiene una lista de todos los slots y busca el primero libre al agregar un nuevo engimono
// tambien gestiona los raycasters de la escena para poder detectar donde esta el puntero (ESTO FUNCIONA DE LA PERRA OK HONESTAMENT NO SÉ SI VAYA A USAR ESTO)
////////////////////////////////////////////////////////////////////////////////////////////

[DisallowMultipleComponent]
public class uiEngimonosInventoryManager : MonoBehaviour
{
    ////////////////////////////////////////////////////////////////////////////////////////////
    // instancia singleton para acceso global
    ////////////////////////////////////////////////////////////////////////////////////////////
    public static uiEngimonosInventoryManager Instance { get; private set; }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // referencias de configuracion
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Referencias")]
    [SerializeField] private Transform slotsParent;
    [SerializeField] private GameObject inventoryItemPrefab;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // opciones adicionales
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Opciones")]
    [SerializeField] private bool cleanChildrenOnSpawn = true;

    private readonly List<InventorySlotUI> slots = new List<InventorySlotUI>();
    private GraphicRaycaster[] raycasters;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // inicializacion principal
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (slotsParent == null)
            Debug.LogError("[InventoryManager] slotsParent no asignado");
        if (inventoryItemPrefab == null)
            Debug.LogError("[InventoryManager] inventoryItemPrefab no asignado");

        slots.Clear();
        if (slotsParent != null)
        {
            foreach (Transform child in slotsParent)
            {
                var slot = child.GetComponent<InventorySlotUI>();
                if (slot != null) slots.Add(slot);
            }
        }

        // no estoy realmente seguro si esta es una buena forma de implementar esto pero funciona asi que lo voy a dejar así
        raycasters = FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None);
        if (raycasters == null || raycasters.Length == 0)
            Debug.LogWarning("[InventoryManager] no hay GraphicRaycaster en escena");
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // agrega un engimono al primer slot libre
    ////////////////////////////////////////////////////////////////////////////////////////////
    public void AddEngimono(EngimonoInstance inst)
    {
        StartCoroutine(AddEngimonoDelayed(inst));
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // corrutina que espera un frame antes de crear el item
    ////////////////////////////////////////////////////////////////////////////////////////////
    private IEnumerator AddEngimonoDelayed(EngimonoInstance inst)
    {
        yield return null;

        if (inventoryItemPrefab == null)
        {
            Debug.LogError("inventoryItemPrefab no asignado");
            yield break;
        }

        var free = GetFirstFreeSlot();
        if (free == null)
        {
            Debug.Log("inventario lleno");
            yield break;
        }

        var go = Instantiate(inventoryItemPrefab, free.transform);
        var itemUI = go.GetComponent<InventoryItemUI>();
        if (itemUI == null)
        {
            Debug.LogError("InventoryItemPrefab no tiene InventoryItemUI");
            Destroy(go);
            yield break;
        }

        itemUI.Setup(inst, free, cleanChildrenOnSpawn);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // obtiene el primer slot libre disponible
    ////////////////////////////////////////////////////////////////////////////////////////////
    public InventorySlotUI GetFirstFreeSlot()
    {
        foreach (var s in slots)
            if (s.IsFree())
                return s;

        return null;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // obtiene el slot que esta bajo el puntero del mouse
    ////////////////////////////////////////////////////////////////////////////////////////////
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