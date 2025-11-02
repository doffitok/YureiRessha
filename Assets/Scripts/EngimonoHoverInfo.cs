using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

[DisallowMultipleComponent]
public class EngimonoHoverInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Prefab de información")]
    [SerializeField] private GameObject infoContainerPrefab;

    [Header("Posiciones (XY en el Canvas)")]
    [SerializeField] private Vector2 offsetTiendaNoComprado = new Vector2(-300f, 0f);
    [SerializeField] private Vector2 offsetTiendaComprado   = new Vector2(0f, -140f);
    [SerializeField] private Vector2 offsetInventario       = new Vector2(0f, -140f);

    [Header("Escala y animación del tooltip")]
    [SerializeField, Min(0.0001f)] private float infoScale = 1f;
    [SerializeField] private float popInSpeed     = 20f;
    [SerializeField] private float popInOvershoot = 1.2f;

    [Header("Animación Hover (Pop Out simplificada)")]
    [SerializeField] private float hoverGrowScale   = 1.12f;
    [SerializeField] private float hoverSettleScale = 1.05f;
    [SerializeField] private float hoverGrowSpeed   = 0.08f;
    [SerializeField] private float hoverSettleSpeed = 0.08f;
    [SerializeField] private float hoverReturnSpeed = 0.08f;

    [Header("Animación aparición inicial")]
    [SerializeField] private float spawnPhase1Speed = 0.08f;
    [SerializeField] private float spawnPhase2Speed = 0.08f;
    [SerializeField] private float spawnPhase1Scale = 1.40f;
    [SerializeField] private float spawnPhase2Scale = 0.80f;

    // 🧩 Variables internas
    private GameObject currentInfoBox;
    private RectTransform rectTransform;
    private Canvas rootCanvas;
    private RectTransform tooltipLayer;
    private bool pointerInside;
    private Coroutine hoverRoutine;
    private Vector3 baseScale;

    // Tipos de engimonos detectados
    private EngimonoShopItem shopItem;
    private InventoryItemUI inventoryItem;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        shopItem      = GetComponent<EngimonoShopItem>();
        inventoryItem = GetComponent<InventoryItemUI>();

        var anyCanvas = GetComponentInParent<Canvas>();
        if (anyCanvas != null)
            rootCanvas = anyCanvas.rootCanvas;

        if (rootCanvas != null)
        {
            var existing = rootCanvas.transform.Find("TooltipLayer");
            tooltipLayer = existing != null ? existing as RectTransform : CreateTooltipLayer(rootCanvas);
        }

        StartCoroutine(DelayedSpawn());
    }

    private IEnumerator DelayedSpawn()
    {
        yield return null; // esperar a que todo se inicialice
        baseScale = rectTransform.localScale;
        yield return SpawnPopInAnimation(baseScale);
    }

    private RectTransform CreateTooltipLayer(Canvas root)
    {
        var go = new GameObject("TooltipLayer", typeof(RectTransform));
        var layer = go.GetComponent<RectTransform>();
        layer.SetParent(root.transform, false);
        layer.anchorMin = Vector2.zero;
        layer.anchorMax = Vector2.one;
        layer.pivot = new Vector2(0.5f, 0.5f);
        layer.offsetMin = layer.offsetMax = Vector2.zero;
        return layer;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    //Esta parte administra las interacciones del mouse con la tienda
    ////////////////////////////////////////////////////////////////////////////////////////////

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInside = true;

        if (hoverRoutine != null)
            StopCoroutine(hoverRoutine);
        hoverRoutine = StartCoroutine(HoverPopOutSequence(true));

        // Detectar si es un Engimono de TIENDA
        if (shopItem != null && shopItem.engimonoData != null)
        {
            CrearTooltipTienda();
        }
        // Detectar si es un Engimono del INVENTARIO
        else if (inventoryItem != null && inventoryItem.instance != null && inventoryItem.instance.data != null)
        {
            CrearTooltipInventario();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;

        if (hoverRoutine != null)
            StopCoroutine(hoverRoutine);
        hoverRoutine = StartCoroutine(HoverPopOutSequence(false));

        StartCoroutine(HideLater());
    }

    private void CrearTooltipTienda()
    {
        if (currentInfoBox != null || infoContainerPrefab == null)
            return;

        var data = shopItem.engimonoData;
        var comprado = shopItem.Comprado;

        currentInfoBox = Instantiate(infoContainerPrefab, tooltipLayer);
        var infoRect = currentInfoBox.GetComponent<RectTransform>();
        infoRect.anchorMin = infoRect.anchorMax = infoRect.pivot = new Vector2(0.5f, 0.5f);

        float s = Mathf.Max(0.0001f, infoScale);
        infoRect.localScale = Vector3.one * (s * 0.85f);

        var nameText = currentInfoBox.transform
            .Find("EngimonoNameContainer/EngimonoNameBox/EngimonoNameText")
            ?.GetComponent<TextMeshProUGUI>();
        var descText = currentInfoBox.transform
            .Find("EngimonoInfoContainer/EngimonoInfoBox/EngimonoInfoText")
            ?.GetComponent<TextMeshProUGUI>();

        if (nameText)
            nameText.text = string.IsNullOrEmpty(data.Nombre) ? "[Sin nombre]" : data.Nombre;
        if (descText)
            descText.text = string.IsNullOrEmpty(data.Descripcion) ? "[Sin descripción]" : data.Descripcion;

        PositionTooltip(infoRect, comprado ? offsetTiendaComprado : offsetTiendaNoComprado);

        var cg = currentInfoBox.GetComponent<CanvasGroup>() ?? currentInfoBox.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.alpha = 1f;

        StartCoroutine(PopInAnimation(infoRect, s));
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    //Esta parte administra las interacciones del mouse con el inventario (DIOS MIO ESTO ESMUY HORRIBLE VOY A MATARME AAAAAAAAAAA)
    ////////////////////////////////////////////////////////////////////////////////////////////

    private void CrearTooltipInventario()
    {
        if (currentInfoBox != null || infoContainerPrefab == null)
            return;

        var data = inventoryItem.instance.data;

        currentInfoBox = Instantiate(infoContainerPrefab, tooltipLayer);
        var infoRect = currentInfoBox.GetComponent<RectTransform>();
        infoRect.anchorMin = infoRect.anchorMax = infoRect.pivot = new Vector2(0.5f, 0.5f);

        float s = Mathf.Max(0.0001f, infoScale);
        infoRect.localScale = Vector3.one * (s * 0.85f);

        var nameText = currentInfoBox.transform
            .Find("EngimonoNameContainer/EngimonoNameBox/EngimonoNameText")
            ?.GetComponent<TextMeshProUGUI>();
        var descText = currentInfoBox.transform
            .Find("EngimonoInfoContainer/EngimonoInfoBox/EngimonoInfoText")
            ?.GetComponent<TextMeshProUGUI>();

        if (nameText)
            nameText.text = string.IsNullOrEmpty(data.Nombre) ? "[Sin nombre]" : data.Nombre;
        if (descText)
            descText.text = string.IsNullOrEmpty(data.Descripcion) ? "[Sin descripción]" : data.Descripcion;

        PositionTooltip(infoRect, offsetInventario);

        var cg = currentInfoBox.GetComponent<CanvasGroup>() ?? currentInfoBox.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.alpha = 1f;

        StartCoroutine(PopInAnimation(infoRect, s));
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // Animaciones y utilidades
    ////////////////////////////////////////////////////////////////////////////////////////////

    private IEnumerator HoverPopOutSequence(bool entering)
    {
        if (baseScale == Vector3.zero)
            baseScale = rectTransform.localScale;

        if (entering)
        {
            Vector3 grow = baseScale * hoverGrowScale;
            Vector3 settle = baseScale * hoverSettleScale;

            yield return AnimateScale(rectTransform, baseScale, grow, hoverGrowSpeed);
            yield return AnimateScale(rectTransform, grow, settle, hoverSettleSpeed);
        }
        else
        {
            yield return AnimateScale(rectTransform, rectTransform.localScale, baseScale, hoverReturnSpeed);
        }
    }

    private IEnumerator SpawnPopInAnimation(Vector3 baseScale)
    {
        rectTransform.localScale = baseScale * spawnPhase1Scale;
        yield return AnimateScale(rectTransform, baseScale * spawnPhase1Scale, baseScale * spawnPhase2Scale, spawnPhase1Speed);
        yield return AnimateScale(rectTransform, baseScale * spawnPhase2Scale, baseScale, spawnPhase2Speed);
    }

    private IEnumerator AnimateScale(RectTransform target, Vector3 from, Vector3 to, float duration)
    {
        if (duration <= 0f)
        {
            target.localScale = to;
            yield break;
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float s = Mathf.SmoothStep(0f, 1f, t);
            target.localScale = Vector3.Lerp(from, to, s);
            yield return null;
        }
        target.localScale = to;
    }

    private void PositionTooltip(RectTransform infoRect, Vector2 offset)
    {
        var cam = (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? rootCanvas.worldCamera : null;

        var screenPos = RectTransformUtility.WorldToScreenPoint(cam, rectTransform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(tooltipLayer, screenPos, cam, out var localPos);
        infoRect.anchoredPosition = localPos + offset;
    }

    private IEnumerator HideLater()
    {
        yield return new WaitForSeconds(0.05f);
        if (!pointerInside && currentInfoBox != null)
        {
            Destroy(currentInfoBox);
            currentInfoBox = null;
        }
    }

    private void OnDisable() => Cleanup(false);
    private void OnDestroy() => Cleanup(true);

    private void Cleanup(bool immediate)
    {
        if (!currentInfoBox) return;
        if (immediate) DestroyImmediate(currentInfoBox);
        else Destroy(currentInfoBox);
        currentInfoBox = null;
    }

    private IEnumerator PopInAnimation(RectTransform infoRect, float baseScale)
    {
        float elapsed = 0f;
        float overshoot = baseScale * popInOvershoot;
        float start = baseScale * 0.85f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * popInSpeed;
            float t = Mathf.SmoothStep(0f, 1f, elapsed);
            float s = Mathf.Lerp(start, overshoot, t);
            infoRect.localScale = new Vector3(s, s, 1f);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * popInSpeed;
            float t = Mathf.SmoothStep(0f, 1f, elapsed);
            float s = Mathf.Lerp(overshoot, baseScale, t);
            infoRect.localScale = new Vector3(s, s, 1f);
            yield return null;
        }

        infoRect.localScale = Vector3.one * baseScale;
    }
}