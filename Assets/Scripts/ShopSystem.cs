using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class ShopSystem : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform shopSlot01;
    [SerializeField] private Transform shopSlot02;
    [SerializeField] private DayLogic dayLogic;

    [Header("Prefabs disponibles para la tienda")]
    [SerializeField] private List<GameObject> availableEngimonos = new List<GameObject>();

    [Header("Tamaño visual")]
    [Tooltip("Tamaño de los engimonos (se aplica igual a X e Y)")]
    [SerializeField] private float Size = 300f;

    [Header("Otras configuraciones")]
    [SerializeField] private bool updateEachDay = true; // si está activo, regenera en ResetDay()

    private const float baseSize = 300f;

    private GameObject currentEngimono1;
    private GameObject currentEngimono2;

    private void Awake()
    {
        if (shopSlot01 == null) shopSlot01 = GameObject.Find("shopSlot01")?.transform;
        if (shopSlot02 == null) shopSlot02 = GameObject.Find("shopSlot02")?.transform;
        if (dayLogic == null) dayLogic = FindObjectOfType<DayLogic>();

        if (dayLogic != null)
        {
            // Escuchar SOLO el reinicio de día
            dayLogic.OnDayReset += OnDayReset;
        }
    }

    private void Start()
    {
        if (shopSlot01 == null || shopSlot02 == null)
        {
            Debug.LogError("[ShopSystem] No se encontraron 'shopSlot01' o 'shopSlot02' en la escena.");
            return;
        }

        if (availableEngimonos.Count == 0)
        {
            Debug.LogWarning("[ShopSystem] No hay Engimonos asignados en la lista de prefabs disponibles.");
            return;
        }

        // Generar una vez al iniciar el juego
        GenerateShopItems();
    }

    private void OnDestroy()
    {
        if (dayLogic != null)
            dayLogic.OnDayReset -= OnDayReset;
    }

    private void OnDayReset()
    {
        if (!updateEachDay) return;
        Debug.Log("[ShopSystem] Día reiniciado → generando nuevos Engimonos");
        GenerateShopItems();
    }

    public void GenerateShopItems()
    {
        if (availableEngimonos.Count == 0) return;

        // Limpiar anteriores
        if (currentEngimono1 != null) Destroy(currentEngimono1);
        if (currentEngimono2 != null) Destroy(currentEngimono2);

        // Seleccionar dos ítems distintos de forma aleatoria
        int index1 = Random.Range(0, availableEngimonos.Count);
        int index2 = index1;
        while (index2 == index1 && availableEngimonos.Count > 1)
            index2 = Random.Range(0, availableEngimonos.Count);

        GameObject prefab1 = availableEngimonos[index1];
        GameObject prefab2 = availableEngimonos[index2];

        currentEngimono1 = Instantiate(prefab1, shopSlot01);
        currentEngimono2 = Instantiate(prefab2, shopSlot02);

        AdjustEngimonoTransform(currentEngimono1, shopSlot01);
        AdjustEngimonoTransform(currentEngimono2, shopSlot02);

        RefrescarVisual(currentEngimono1);
        RefrescarVisual(currentEngimono2);

        Debug.Log($"[ShopSystem] Nuevos Engimonos: {prefab1.name} y {prefab2.name}");
    }

    private void AdjustEngimonoTransform(GameObject go, Transform parentSlot)
    {
        if (go == null) return;

        float uniformWorldScale = Size / baseSize;
        Vector3 desiredWorldScale = new Vector3(uniformWorldScale, uniformWorldScale, uniformWorldScale);

        Vector3 parentLossy = parentSlot != null ? parentSlot.lossyScale : Vector3.one;
        Vector3 safeParentLossy = new Vector3(
            Mathf.Approximately(parentLossy.x, 0f) ? 1f : parentLossy.x,
            Mathf.Approximately(parentLossy.y, 0f) ? 1f : parentLossy.y,
            Mathf.Approximately(parentLossy.z, 0f) ? 1f : parentLossy.z
        );

        Vector3 localScaleToApply = new Vector3(
            desiredWorldScale.x / safeParentLossy.x,
            desiredWorldScale.y / safeParentLossy.y,
            desiredWorldScale.z / safeParentLossy.z
        );

        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = localScaleToApply;
    }

    private void RefrescarVisual(GameObject engimono)
    {
        if (engimono == null) return;
        var shopItem = engimono.GetComponent<EngimonoShopItem>();
        if (shopItem != null)
            shopItem.SendMessage("ActualizarUI", SendMessageOptions.DontRequireReceiver);

        foreach (var img in engimono.GetComponentsInChildren<Image>(true)) img.enabled = true;
        foreach (var txt in engimono.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            txt.enabled = true;
            txt.alpha = 1f;
        }
    }
}