using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

[DisallowMultipleComponent]
public class EngimonoHoverInfo_Shop : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [Header("Prefab de información")]
    [SerializeField] private GameObject infoContainerPrefab;

    [Header("Posiciones (XY en el Canvas)")]
    [SerializeField] private Vector2 offsetNoComprado = new Vector2(-300f, 0f);
    [SerializeField] private Vector2 offsetComprado = new Vector2(0f, -140f);

    [Header("Escala y animación")]
    [SerializeField, Min(0.0001f)] private float infoScale = 1f;
    [SerializeField] private float popInSpeed = 20f;
    [SerializeField] private float popInOvershoot = 1.2f;

    private GameObject currentInfoBox;
    private RectTransform rectTransform;
    private Canvas rootCanvas;
    private RectTransform tooltipLayer;
    private EngimonoShopItem shopItem;
    private Coroutine popInRoutine;

    // Última posición del puntero (pantalla)
    private Vector2 lastPointerScreenPos;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        shopItem = GetComponent<EngimonoShopItem>();

        // Root canvas del engimono actual (tienda o inventario)
        var anyCanvas = GetComponentInParent<Canvas>();
        if (anyCanvas != null)
            rootCanvas = anyCanvas.rootCanvas;

        if (rootCanvas != null)
        {
            // Busca/crea la capa de tooltips dentro del root canvas actual
            var existing = rootCanvas.transform.Find("TooltipLayer");
            tooltipLayer = existing ? existing as RectTransform : CreateTooltipLayer(rootCanvas);
        }
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

        // Mantener esta capa al frente dentro del root canvas
        layer.SetAsLastSibling();
        return layer;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        lastPointerScreenPos = eventData != null ? eventData.position : (Vector2)Input.mousePosition;

        if (currentInfoBox != null ||
            infoContainerPrefab == null ||
            shopItem == null ||
            shopItem.engimonoData == null ||
            rootCanvas == null ||
            tooltipLayer == null)
            return;

        var data = shopItem.engimonoData;
        var comprado = shopItem.Comprado;

        // Crear cuadro en la capa de tooltips del root canvas correspondiente
        currentInfoBox = Instantiate(infoContainerPrefab, tooltipLayer);
        var infoRect = currentInfoBox.GetComponent<RectTransform>();
        infoRect.anchorMin = infoRect.anchorMax = infoRect.pivot = new Vector2(0.5f, 0.5f);

        float s = Mathf.Max(0.0001f, infoScale);
        infoRect.localScale = new Vector3(s * 0.85f, s * 0.85f, 1f);

        // Traer la capa de tooltips al frente por si hay otros paneles encima
        tooltipLayer.SetAsLastSibling();

        // Textos
        var nameText = currentInfoBox.transform
            .Find("EngimonoNameContainer/EngimonoNameBox/EngimonoNameText")
            ?.GetComponent<TextMeshProUGUI>();

        var descText = currentInfoBox.transform
            .Find("EngimonoInfoContainer/EngimonoInfoBox/EngimonoInfoText")
            ?.GetComponent<TextMeshProUGUI>();

        if (nameText) nameText.text = string.IsNullOrEmpty(data.Nombre) ? "[Sin nombre]" : data.Nombre;
        if (descText) descText.text = string.IsNullOrEmpty(data.Descripcion) ? "[Sin descripción]" : data.Descripcion;

        // Posicionar por puntero (robusto para tienda e inventario)
        PositionTooltipByPointer(infoRect, comprado);

        // Evitar bloquear raycasts
        var cg = currentInfoBox.GetComponent<CanvasGroup>();
        if (cg == null) cg = currentInfoBox.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        // Animación
        popInRoutine = StartCoroutine(PopInAnimation(infoRect, s));
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        lastPointerScreenPos = eventData != null ? eventData.position : (Vector2)Input.mousePosition;

        // Si el tooltip está activo, acompañarlo suavemente (opcional, aquí lo “pegamos” al puntero)
        if (currentInfoBox != null)
        {
            var infoRect = currentInfoBox.GetComponent<RectTransform>();
            if (infoRect != null)
            {
                bool comprado = shopItem != null && shopItem.Comprado;
                PositionTooltipByPointer(infoRect, comprado);
            }
        }
    }

    private void PositionTooltipByPointer(RectTransform infoRect, bool comprado)
    {
        if (infoRect == null || rootCanvas == null || tooltipLayer == null) return;

        // Cámara según el root canvas donde está el TooltipLayer
        Camera camForTooltip =
            (rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? rootCanvas.worldCamera : null;

        // Convertir la posición del puntero (pantalla) al espacio local del TooltipLayer
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tooltipLayer, lastPointerScreenPos, camForTooltip, out var localPos))
        {
            Vector2 offset = comprado ? offsetComprado : offsetNoComprado;
            infoRect.anchoredPosition = localPos + offset;
        }
    }

    private IEnumerator PopInAnimation(RectTransform infoRect, float baseScale)
    {
        float elapsed = 0f;
        float overshoot = baseScale * popInOvershoot;
        float start = baseScale * 0.85f;

        while (elapsed < 1f)
        {
            if (infoRect == null) yield break;
            elapsed += Time.deltaTime * popInSpeed;
            float t = Mathf.SmoothStep(0f, 1f, elapsed);
            float s = Mathf.Lerp(start, overshoot, t);
            infoRect.localScale = new Vector3(s, s, 1f);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < 1f)
        {
            if (infoRect == null) yield break;
            elapsed += Time.deltaTime * popInSpeed;
            float t = Mathf.SmoothStep(0f, 1f, elapsed);
            float s = Mathf.Lerp(overshoot, baseScale, t);
            infoRect.localScale = new Vector3(s, s, 1f);
            yield return null;
        }

        if (infoRect != null)
            infoRect.localScale = Vector3.one * baseScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Cortar animación y destruir al instante
        if (popInRoutine != null)
        {
            StopCoroutine(popInRoutine);
            popInRoutine = null;
        }

        if (currentInfoBox != null)
        {
            Destroy(currentInfoBox);
            currentInfoBox = null;
        }
    }

    private void OnDisable() => Cleanup(false);
    private void OnDestroy() => Cleanup(true);

    private void Cleanup(bool immediate)
    {
        if (popInRoutine != null)
        {
            StopCoroutine(popInRoutine);
            popInRoutine = null;
        }

        if (!currentInfoBox) return;
        if (immediate) DestroyImmediate(currentInfoBox);
        else Destroy(currentInfoBox);
        currentInfoBox = null;
    }
}