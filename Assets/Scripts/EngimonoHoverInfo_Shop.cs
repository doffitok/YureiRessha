using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

[DisallowMultipleComponent]
public class EngimonoHoverInfo_Shop : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
    private bool pointerInside;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        shopItem = GetComponent<EngimonoShopItem>();

        var anyCanvas = GetComponentInParent<Canvas>();
        if (anyCanvas != null) rootCanvas = anyCanvas.rootCanvas;

        if (rootCanvas != null)
        {
            var existing = rootCanvas.transform.Find("TooltipLayer");
            tooltipLayer = existing != null ? existing as RectTransform : CreateTooltipLayer(rootCanvas);
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
        return layer;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInside = true;

        // 🧩 Debug para detectar causas
        Debug.Log("[ShopHover] Intentando crear tooltip...");
        Debug.Log($"  currentInfoBox: {(currentInfoBox ? "EXISTE" : "null")}");
        Debug.Log($"  infoContainerPrefab: {(infoContainerPrefab ? "ASIGNADO" : "null")}");
        Debug.Log($"  shopItem: {(shopItem ? "ASIGNADO" : "null")}");
        Debug.Log($"  data: {(shopItem?.engimonoData != null ? "ASIGNADO" : "null")}");

        if (currentInfoBox != null ||
            infoContainerPrefab == null ||
            shopItem == null ||
            shopItem.engimonoData == null)
        {
            Debug.LogWarning("[ShopHover] Cancelado: alguna referencia está en null.");
            return;
        }

        var data = shopItem.engimonoData;
        var comprado = shopItem.Comprado;

        // ✅ Crear cuadro en el canvas
        currentInfoBox = Instantiate(infoContainerPrefab, tooltipLayer);
        var infoRect = currentInfoBox.GetComponent<RectTransform>();
        infoRect.anchorMin = infoRect.anchorMax = infoRect.pivot = new Vector2(0.5f, 0.5f);

        float s = Mathf.Max(0.0001f, infoScale);
        infoRect.localScale = new Vector3(s * 0.85f, s * 0.85f, 1f);

        // ✅ Buscar textos con las mismas rutas que el prefab funcional
        var nameText = currentInfoBox.transform
            .Find("EngimonoNameContainer/EngimonoNameBox/EngimonoNameText")
            ?.GetComponent<TextMeshProUGUI>();

        var descText = currentInfoBox.transform
            .Find("EngimonoInfoContainer/EngimonoInfoBox/EngimonoInfoText")
            ?.GetComponent<TextMeshProUGUI>();

        // 🧠 Si no los encuentra, imprime la jerarquía
        if (!nameText || !descText)
        {
            Debug.LogWarning("[ShopHover] No se encontraron los textos dentro del prefab. Imprimiendo jerarquía:");
            foreach (Transform child in currentInfoBox.GetComponentsInChildren<Transform>(true))
                Debug.Log(" - " + child.name);
        }

        // ✅ Asignar textos correctamente
        if (nameText)
        {
            nameText.text = string.IsNullOrEmpty(data.Nombre) ? "[Sin nombre]" : data.Nombre;
            nameText.ForceMeshUpdate(true);
            Debug.Log($"[ShopHover] Texto asignado al nombre: {nameText.text}");
        }

        if (descText)
        {
            descText.text = string.IsNullOrEmpty(data.Descripcion) ? "[Sin descripción]" : data.Descripcion;
            descText.ForceMeshUpdate(true);
        }

        // Posicionar
        PositionTooltip(infoRect, comprado);

        // Evitar bloqueos de raycast (muy importante)
        var cg = currentInfoBox.GetComponent<CanvasGroup>();
        if (cg == null) cg = currentInfoBox.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        StartCoroutine(PopInAnimation(infoRect, s));
    }

    private void PositionTooltip(RectTransform infoRect, bool comprado)
    {
        var cam = (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? rootCanvas.worldCamera : null;

        var screenPos = RectTransformUtility.WorldToScreenPoint(cam, rectTransform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(tooltipLayer, screenPos, cam, out var localPos);
        Vector2 offset = comprado ? offsetComprado : offsetNoComprado;
        infoRect.anchoredPosition = localPos + offset;
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

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
        StartCoroutine(HideLater());
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
}