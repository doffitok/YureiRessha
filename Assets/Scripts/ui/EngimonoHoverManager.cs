using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class EngimonoHoverManager : MonoBehaviour
{
    [Header("Prefab del Tooltip")]
    [SerializeField] private GameObject infoContainerPrefab;

    [Header("Offsets de posición")]
    [SerializeField] private Vector2 offsetTiendaNoComprado = new Vector2(-300f, 0f);
    [SerializeField] private Vector2 offsetTiendaComprado = new Vector2(0f, -140f);
    [SerializeField] private Vector2 offsetInventario = new Vector2(0f, -140f);

    [Header("Escala y animación")]
    [SerializeField, Min(0.001f)] private float infoScale = 1f;
    [SerializeField] private float popInSpeed = 20f;
    [SerializeField] private float popInOvershoot = 1.2f;

    [Header("Depuración")]
    [SerializeField] private bool debugLogs = false;

    private Canvas rootCanvas;
    private RectTransform tooltipLayer;
    private GameObject currentTooltip;
    private GameObject currentTarget;

    private void Awake()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            rootCanvas = canvas.rootCanvas;
            var existing = rootCanvas.transform.Find("TooltipLayer");
            tooltipLayer = existing ? existing.GetComponent<RectTransform>() : CreateTooltipLayer(rootCanvas);
        }
    }

    private void Update()
    {
        if (Mouse.current == null) return;
        Vector2 mousePos = Mouse.current.position.ReadValue();

        var pointer = new PointerEventData(EventSystem.current) { position = mousePos };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);

        GameObject hovered = null;
        foreach (var r in results)
        {
            // Ahora detecta directamente objetos con EngimonoItem
            if (r.gameObject.GetComponent<EngimonoItem>())
            {
                hovered = r.gameObject;
                break;
            }
        }

        if (hovered != currentTarget)
        {
            if (hovered == null)
                HideTooltip();
            else
                ShowTooltip(hovered);

            currentTarget = hovered;
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

    private void ShowTooltip(GameObject target)
    {
        if (infoContainerPrefab == null || tooltipLayer == null)
            return;

        HideTooltip();

        // Datos a mostrar
        string nombre = "[Sin nombre]";
        string descripcion = "[Sin descripción]";
        Vector2 offset = Vector2.zero;

        // Obtener el EngimonoItem directamente
        var item = target.GetComponent<EngimonoItem>();

        if (item != null)
        {
            nombre = item.Nombre;
            descripcion = item.Descripcion;

            // Decidir offset según dónde está (tienda o inventario)
            // Si el objeto tiene padre con "shop" en el nombre → tienda
            Transform t = target.transform;
            bool esTienda = false;
            while (t != null)
            {
                if (t.name.ToLower().Contains("shop"))
                {
                    esTienda = true;
                    break;
                }
                t = t.parent;
            }

            if (esTienda)
                offset = item.Comprado ? offsetTiendaComprado : offsetTiendaNoComprado;
            else
                offset = offsetInventario;
        }

        // Crear tooltip
        currentTooltip = Instantiate(infoContainerPrefab, tooltipLayer);
        var rect = currentTooltip.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);

        // Llenar textos
        var nameText = currentTooltip.transform.Find("EngimonoNameContainer/EngimonoNameBox/EngimonoNameText")?.GetComponent<TextMeshProUGUI>();
        var descText = currentTooltip.transform.Find("EngimonoInfoContainer/EngimonoInfoBox/EngimonoInfoText")?.GetComponent<TextMeshProUGUI>();

        if (nameText) nameText.text = nombre;
        if (descText) descText.text = descripcion;

        // CanvasGroup para evitar raycasts
        var cg = currentTooltip.GetComponent<CanvasGroup>() ?? currentTooltip.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.alpha = 1f;

        PositionTooltip(rect, target.transform as RectTransform, offset);
        StartCoroutine(PopIn(rect, infoScale));

        if (debugLogs)
            Debug.Log($"[HoverManager] Tooltip creado → {nombre}");
    }

    private void PositionTooltip(RectTransform tooltipRect, RectTransform targetRect, Vector2 offset)
    {
        if (rootCanvas == null || tooltipRect == null || targetRect == null)
            return;

        Camera cam = (rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? rootCanvas.worldCamera : null;

        Vector3 worldCenter = targetRect.TransformPoint(targetRect.rect.center);
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldCenter);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(tooltipLayer, screenPos, cam, out var localPos);
        tooltipRect.anchoredPosition = localPos + offset;
    }

    private System.Collections.IEnumerator PopIn(RectTransform rect, float scale)
    {
        if (rect == null) yield break;

        float elapsed = 0f;
        float start = scale * 0.8f;
        float end = scale * popInOvershoot;

        while (elapsed < 1f)
        {
            if (rect == null) yield break;
            elapsed += Time.deltaTime * popInSpeed;
            float t = Mathf.SmoothStep(0f, 1f, elapsed);
            float s = Mathf.Lerp(start, end, t);
            rect.localScale = Vector3.one * s;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < 1f)
        {
            if (rect == null) yield break;
            elapsed += Time.deltaTime * popInSpeed;
            float t = Mathf.SmoothStep(0f, 1f, elapsed);
            float s = Mathf.Lerp(end, scale, t);
            rect.localScale = Vector3.one * s;
            yield return null;
        }

        if (rect != null)
            rect.localScale = Vector3.one * scale;
    }

    private void HideTooltip()
    {
        if (currentTooltip != null)
            Destroy(currentTooltip);
        currentTooltip = null;
    }
}