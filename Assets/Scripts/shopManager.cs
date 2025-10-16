using UnityEngine;
using System.Collections.Generic;

public class ShopSystem : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform shopSlot01;
    [SerializeField] private Transform shopSlot02;
    [SerializeField] private DayLogic dayLogic;  // Referencia al script del día

    [Header("Prefabs disponibles para la tienda")]
    [SerializeField] private List<GameObject> availableEngimonos = new List<GameObject>();

    [Header("Configuración del tamaño de los engimonos")]
    [Tooltip("Tamaño de los engimonos en unidades o píxeles (aplica igual a X e Y)")]
    [SerializeField] private float Size = 300f;

    [Header("Otras configuraciones")]
    [SerializeField] private bool updateEachDay = true; // Cambiar automáticamente cada día

    private int lastDayValue = -1;
    private GameObject currentPrefab;
    private GameObject currentEngimono1;
    private GameObject currentEngimono2;

    void Start()
    {
        // Buscar los objetos si no fueron asignados
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

        GenerateShopItemForDay(); // Generar al iniciar
    }

    void Update()
    {
        if (!updateEachDay || dayLogic == null)
            return;

        // Detectar si el día cambió (cuando currentSecond se resetea)
        if (dayLogic.currentSecond < 1 && lastDayValue != 0)
        {
            GenerateShopItemForDay();
            lastDayValue = 0;
        }

        // Guardar el valor actual para comparar después
        lastDayValue = dayLogic.currentSecond;
    }

    public void GenerateShopItemForDay()
    {
        // Limpia los anteriores si existían
        if (currentEngimono1 != null) Destroy(currentEngimono1);
        if (currentEngimono2 != null) Destroy(currentEngimono2);

        if (availableEngimonos.Count == 0)
            return;

        // Selecciona un prefab (rotativo según día)
        int dayIndex = dayLogic != null ? dayLogic.currentSecond % availableEngimonos.Count : 0;
        currentPrefab = availableEngimonos[dayIndex];

        // Instancia el mismo prefab en ambos slots
        currentEngimono1 = Instantiate(currentPrefab, shopSlot01);
        currentEngimono2 = Instantiate(currentPrefab, shopSlot02);

        // Ajusta tamaño y posición
        AdjustEngimonoTransform(currentEngimono1);
        AdjustEngimonoTransform(currentEngimono2);

        Debug.Log($"[ShopSystem] Día actualizado: mostrando {currentPrefab.name} en ambos slots.");
    }

    private void AdjustEngimonoTransform(GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            // Prefab con UI
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.anchoredPosition = Vector2.zero;

            rt.sizeDelta = new Vector2(Size, Size); // aplica la escala custom
            rt.localScale = Vector3.one;
        }
        else
        {
            // Prefab con SpriteRenderer
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                float spriteWidth = sr.sprite.bounds.size.x;
                float spriteHeight = sr.sprite.bounds.size.y;

                float scaleX = Size / spriteWidth;
                float scaleY = Size / spriteHeight;

                go.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }
            else
            {
                go.transform.localScale = Vector3.one;
            }

            go.transform.localPosition = Vector3.zero;
        }
    }
}