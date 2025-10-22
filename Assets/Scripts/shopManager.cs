using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class ShopSystem : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform shopSlot01;
    [SerializeField] private Transform shopSlot02;
    [SerializeField] private DayLogic dayLogic;  // Referencia al script del día

    [Header("Prefabs disponibles para la tienda")]
    [SerializeField] private List<GameObject> availableEngimonos = new List<GameObject>();

    [Header("Configuración del tamaño de los engimonos")]
    [Tooltip("Tamaño de los engimonos (se aplica igual a X e Y)")]
    [SerializeField] private float Size = 300f;

    [Header("Otras configuraciones")]
    [SerializeField] private bool updateEachDay = true; // Cambiar automáticamente cada día

    // Tamaño base (usa el mismo valor que tomabas como referencia antes).
    // Cambia si tus prefabs están diseñados para otra 'unidad base'.
    private const float baseSize = 300f;

    private int lastDayValue = -1;
    private GameObject currentPrefab;
    private GameObject currentEngimono1;
    private GameObject currentEngimono2;

    void Start()
    {
        if (shopSlot01 == null) shopSlot01 = GameObject.Find("shopSlot01")?.transform;
        if (shopSlot02 == null) shopSlot02 = GameObject.Find("shopSlot02")?.transform;
        if (dayLogic == null) dayLogic = FindObjectOfType<DayLogic>();

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

        GenerateShopItemForDay();
    }

    void Update()
    {
        if (!updateEachDay || dayLogic == null) return;

        // Detectar cambio de día (cuando currentSecond reinicia o cambia)
        if (dayLogic.currentSecond < 1 && lastDayValue != 0)
        {
            GenerateShopItemForDay();
            lastDayValue = 0;
        }

        lastDayValue = dayLogic.currentSecond;
    }

    public void GenerateShopItemForDay()
    {
        if (currentEngimono1 != null) Destroy(currentEngimono1);
        if (currentEngimono2 != null) Destroy(currentEngimono2);
        if (availableEngimonos.Count == 0) return;

        int dayIndex = dayLogic != null ? dayLogic.currentSecond % availableEngimonos.Count : 0;
        currentPrefab = availableEngimonos[dayIndex];

        // Instanciar en los slots como hijos (para heredar posición) pero corregiremos la escala local
        currentEngimono1 = Instantiate(currentPrefab, shopSlot01);
        currentEngimono2 = Instantiate(currentPrefab, shopSlot02);

        AdjustEngimonoTransform(currentEngimono1, shopSlot01);
        AdjustEngimonoTransform(currentEngimono2, shopSlot02);

        RefrescarPrecio(currentEngimono1);
        RefrescarPrecio(currentEngimono2);

        Debug.Log($"[ShopSystem] Día actualizado: mostrando {currentPrefab.name} en ambos slots.");
    }

    /// <summary>
    /// Ajusta transform del engimono para que el tamaño final (world / lossy) coincida con Size,
    /// sin importar la escala del padre (shopSlot pequeño).
    /// </summary>
    private void AdjustEngimonoTransform(GameObject go, Transform parentSlot)
    {
        if (go == null) return;

        // Escala global objetivo (world scale deseada)
        float uniformWorldScale = Size / baseSize; // p.ej. 300->1, 600->2, etc.
        Vector3 desiredWorldScale = new Vector3(uniformWorldScale, uniformWorldScale, uniformWorldScale);

        // Obtener la escala global (lossy) del padre (slot) — si el padre tiene escala muy pequeña, la usamos
        Vector3 parentLossy = parentSlot != null ? parentSlot.lossyScale : Vector3.one;

        // Protegemos contra 0 para evitar NaN/infinito
        Vector3 safeParentLossy = new Vector3(
            Mathf.Approximately(parentLossy.x, 0f) ? 1f : parentLossy.x,
            Mathf.Approximately(parentLossy.y, 0f) ? 1f : parentLossy.y,
            Mathf.Approximately(parentLossy.z, 0f) ? 1f : parentLossy.z
        );

        // Calcular localScale necesario para obtener desiredWorldScale cuando se multiplique por parentLossy
        Vector3 localScaleToApply = new Vector3(
            desiredWorldScale.x / safeParentLossy.x,
            desiredWorldScale.y / safeParentLossy.y,
            desiredWorldScale.z / safeParentLossy.z
        );

        // Si el prefab tiene RectTransform (UI inside Canvas)
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            // Aseguramos posición dentro del slot y rotación neutra
            rt.anchoredPosition = Vector2.zero;
            rt.localRotation = Quaternion.identity;

            // Aplicar la localScale calculada (esto compensa la escala del padre y produce la escala global deseada)
            go.transform.localScale = localScaleToApply;

            // Aplicar posiciones/rotaciones de hijos si existen (mantener tus valores específicos)
            Transform tag = go.transform.Find("shopSlotPriceTag");
            if (tag != null)
            {
                tag.localPosition = new Vector3(67.5f, -48f, 0f);
                tag.localEulerAngles = new Vector3(1.379f, 1.379f, 188.31f);
                tag.localScale = new Vector3(1.3358f, 1.3358f, 1.3358f);
            }

            Transform price = go.transform.Find("shopSlotPriceTag/shopSlotPrice");
            if (price != null)
            {
                price.localPosition = new Vector3(-26.36086f, 1.320324f, 0.8764998f);
                price.localEulerAngles = new Vector3(1.572f, 1.175f, 179.417f);
                price.localScale = new Vector3(0.748615f, 0.748615f, 0.748615f);
            }

            // Asegurar visibilidad de todos los hijos UI
            foreach (var child in go.GetComponentsInChildren<RectTransform>(true))
            {
                child.gameObject.SetActive(true);
            }
        }
        else
        {
            // Si no es UI (por si acaso), aplicamos escala y posición normal
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = localScaleToApply;
        }

        // Debug: comprueba la escala resultante (lossy) para verificar que quedo como se esperaba
        Vector3 resultLossy = go.transform.lossyScale;
        Debug.Log($"[ShopSystem] Ajuste '{go.name}' -> localScale aplicada {localScaleToApply}, lossyScale resultante {resultLossy}, parentLossy {parentLossy}");
    }

    private void RefrescarPrecio(GameObject engimono)
    {
        if (engimono == null) return;

        var display = engimono.GetComponent<engimonoPrice>();
        if (display != null) display.MostrarPrecio();

        // Asegurar visibilidad de los componentes visuales
        foreach (var img in engimono.GetComponentsInChildren<Image>(true)) img.enabled = true;
        foreach (var txt in engimono.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            txt.enabled = true;
            txt.alpha = 1f;
        }

        Debug.Log($"[ShopSystem] Hijos visuales detectados en {engimono.name}: {engimono.GetComponentsInChildren<Transform>(true).Length}");
    }
}