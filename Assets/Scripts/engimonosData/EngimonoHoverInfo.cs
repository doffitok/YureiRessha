using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

////////////////////////////////////////////////////////////////////////////////////////////
// hover de engimonos con tooltip animado que aparece mostrando el nombre y descripcion del engimono
//
// autodetecta si el contexto es tienda o inventario porque el engimono de inventario en realidad es un objeto nulo al que se le asigna la info del comprado
// genuinamente no se si sea la mejor forma de hacerlo o mas optimizado pero es lo que me sirvio y no pienso hacerelo de nuevo
// spawnea una capa de tooltip en el root canvas y coloca la cajita cerca del item con offsets (porque si no se spawnea muy cerca o muy lejos)
// tiene animaciones de pop in y efectos de hover
// POR CIERTO ESTE SCRIPT ES ASI COMO HORRIBLE??? Genuinamente no tengo idea de como esto funciona
////////////////////////////////////////////////////////////////////////////////////////////

[DisallowMultipleComponent]
public class EngimonoHoverInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    ////////////////////////////////////////////////////////////////////////////////////////////
    // configuracion general y depuracion
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Depuracion")]
    [SerializeField] private bool debugLogs = true;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // prefab base del tooltip y offsets por contexto
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Prefab de informacion")]
    [SerializeField] private GameObject infoContainerPrefab;

    [Header("Posiciones XY en el Canvas")]
    [SerializeField] private Vector2 offsetTiendaNoComprado = new Vector2(-300f, 0f);
    [SerializeField] private Vector2 offsetTiendaComprado = new Vector2(0f, -140f);
    [SerializeField] private Vector2 offsetInventario = new Vector2(0f, -140f);

    ////////////////////////////////////////////////////////////////////////////////////////////
    // configuracion de escala y animaciones varias del tooltip
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Escala y animacion del tooltip")]
    [SerializeField, Min(0.0001f)] private float infoScale = 1f;
    [SerializeField] private float popInSpeed = 20f;
    [SerializeField] private float popInOvershoot = 1.2f;

    [Header("Animacion hover pop out")]
    [SerializeField] private float hoverGrowScale = 1.12f;
    [SerializeField] private float hoverSettleScale = 1.05f;
    [SerializeField] private float hoverGrowSpeed = 0.08f;
    [SerializeField] private float hoverSettleSpeed = 0.08f;
    [SerializeField] private float hoverReturnSpeed = 0.08f;

    [Header("Animacion aparicion inicial")]
    [SerializeField] private float spawnPhase1Speed = 0.08f;
    [SerializeField] private float spawnPhase2Speed = 0.08f;
    [SerializeField] private float spawnPhase1Scale = 1.40f;
    [SerializeField] private float spawnPhase2Scale = 0.80f;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // internos de estado y referencias
    ////////////////////////////////////////////////////////////////////////////////////////////
    private GameObject currentInfoBox;
    private RectTransform rectTransform;
    private Canvas rootCanvas;
    private RectTransform tooltipLayer;
    private bool pointerInside;
    private Coroutine hoverRoutine;
    private Vector3 baseScale;

    // tipos detectados
    private EngimonoShopItem shopItem;
    private InventoryItemUI inventoryItem;

    private string Tag => $"[HoverInfo] ({gameObject.name}) ";

    private void Log(string msg)
    {
        if (debugLogs) Debug.Log(Tag + msg, this);
    }
    private void Warn(string msg)
    {
        if (debugLogs) Debug.LogWarning(Tag + msg, this);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // awake inicializa referencias y prepara la capa de tooltips
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        shopItem = GetComponent<EngimonoShopItem>();
        inventoryItem = GetComponent<InventoryItemUI>();

        var anyCanvas = GetComponentInParent<Canvas>();
        if (anyCanvas != null)
            rootCanvas = anyCanvas.rootCanvas;

        if (rootCanvas != null)
        {
            var existing = rootCanvas.transform.Find("TooltipLayer");
            tooltipLayer = existing != null ? existing as RectTransform : CreateTooltipLayer(rootCanvas);
        }

        DetectContext();
        StartCoroutine(DelayedSpawn());
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // detecta si esto vive en tienda o inventario y se adapta
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void DetectContext()
    {
        if (inventoryItem != null)
        {
            Log("contexto detectado inventario");
            return;
        }

        if (shopItem != null)
        {
            Transform t = transform;
            while (t != null)
            {
                if (t.name.ToLower().Contains("inventory") || t.GetComponent<InventoryItemUI>() != null)
                {
                    inventoryItem = t.GetComponent<InventoryItemUI>();
                    shopItem = null;
                    Log("contexto corregido a inventario por jerarquia");
                    return;
                }
                t = t.parent;
            }

            if (shopItem.Comprado)
            {
                Log("contexto corregido a inventario por comprado");
                inventoryItem = GetComponent<InventoryItemUI>();
                shopItem = null;
                return;
            }

            Log("contexto detectado tienda");
            return;
        }

        Log("contexto desconocido sin componentes conocidos");
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // animacion inicial del propio item para que no aparezca rigido
    ////////////////////////////////////////////////////////////////////////////////////////////
    private IEnumerator DelayedSpawn()
    {
        // no se por que un frame extra arregla esto pero lo hace asi que lo dejamos
        yield return null;

        baseScale = rectTransform.localScale;
        yield return SpawnPopInAnimation(baseScale);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // crea una capa full screen para alojar los tooltips
    ////////////////////////////////////////////////////////////////////////////////////////////
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
    // entrada y salida del puntero
    ////////////////////////////////////////////////////////////////////////////////////////////
    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInside = true;
        if (hoverRoutine != null)
            StopCoroutine(hoverRoutine);
        hoverRoutine = StartCoroutine(HoverPopOutSequence(true));

        DetectContext();

        if (shopItem != null)
        {
            if (shopItem.Comprado)
            {
                Log("pointer enter tienda comprado se trata como inventario");
                StartCoroutine(EsperarYCrearTooltipInventario());
                return;
            }

            Log("pointer enter contexto tienda");
            StartCoroutine(EsperarYCrearTooltipTienda());
        }
        else if (inventoryItem != null)
        {
            Log("pointer enter contexto inventario");
            StartCoroutine(EsperarYCrearTooltipInventario());
        }
        else
        {
            Warn("pointer enter contexto desconocido");
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

    ////////////////////////////////////////////////////////////////////////////////////////////
    // espera datos de tienda si aun no existen y luego crea tooltip
    ////////////////////////////////////////////////////////////////////////////////////////////
    private IEnumerator EsperarYCrearTooltipTienda()
    {
        int frames = 0;
        while (shopItem != null && shopItem.engimonoData == null && frames++ < 100)
            yield return null;

        if (shopItem == null || shopItem.engimonoData == null)
        {
            Warn("tooltip cancelado por datos nulos en tienda");
            yield break;
        }

        CrearTooltip(
            shopItem.engimonoData.Nombre,
            shopItem.engimonoData.Descripcion,
            shopItem.Comprado ? offsetTiendaComprado : offsetTiendaNoComprado
        );
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // espera datos de inventario si aun no existen y luego crea tooltip
    ////////////////////////////////////////////////////////////////////////////////////////////
    private IEnumerator EsperarYCrearTooltipInventario()
    {
        int frames = 0;
        while (inventoryItem != null &&
               (inventoryItem.instance == null || inventoryItem.instance.data == null) &&
               frames++ < 100)
            yield return null;

        if (inventoryItem == null || inventoryItem.instance == null || inventoryItem.instance.data == null)
        {
            Warn("tooltip cancelado por datos nulos en inventario");
            yield break;
        }

        CrearTooltip(
            inventoryItem.instance.data.Nombre,
            inventoryItem.instance.data.Descripcion,
            offsetInventario
        );
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // crea el tooltip y rellena textos posicion y canvas group
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void CrearTooltip(string nombre, string descripcion, Vector2 offset)
    {
        if (currentInfoBox != null || infoContainerPrefab == null)
            return;

        currentInfoBox = Instantiate(infoContainerPrefab, tooltipLayer);
        var infoRect = currentInfoBox.GetComponent<RectTransform>();
        infoRect.anchorMin = infoRect.anchorMax = infoRect.pivot = new Vector2(0.5f, 0.5f);

        float s = Mathf.Max(0.0001f, infoScale);
        infoRect.localScale = Vector3.one * (s * 0.85f);

        var nameText = currentInfoBox.transform.Find("EngimonoNameContainer/EngimonoNameBox/EngimonoNameText")?.GetComponent<TextMeshProUGUI>();
        var descText = currentInfoBox.transform.Find("EngimonoInfoContainer/EngimonoInfoBox/EngimonoInfoText")?.GetComponent<TextMeshProUGUI>();

        if (nameText) nameText.text = string.IsNullOrEmpty(nombre) ? "[Sin nombre]" : nombre; // si el diseno viene vacio muestra algo legible
        if (descText) descText.text = string.IsNullOrEmpty(descripcion) ? "[Sin descripcion]" : descripcion;

        Log($"crear tooltip nombre {nombre} descripcion {descripcion} offset {offset}");

        PositionTooltip(infoRect, offset);

        var cg = currentInfoBox.GetComponent<CanvasGroup>() ?? currentInfoBox.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.alpha = 1f;

        StartCoroutine(PopInAnimation(infoRect, s));
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // animaciones de hover y de spawn para el item y para el tooltip
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

    ////////////////////////////////////////////////////////////////////////////////////////////
    // calcula la posicion del tooltip en coordenadas de la capa
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void PositionTooltip(RectTransform infoRect, Vector2 offset)
    {
        var cam = (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? rootCanvas.worldCamera : null;

        var screenPos = RectTransformUtility.WorldToScreenPoint(cam, rectTransform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(tooltipLayer, screenPos, cam, out var localPos);
        infoRect.anchoredPosition = localPos + offset;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // oculta el tooltip un ratito despues de salir para evitar parpadeos asquerosos (no tengo idea por que pasa esto pero ok)
    ////////////////////////////////////////////////////////////////////////////////////////////
    private IEnumerator HideLater()
    {
        yield return new WaitForSeconds(0.05f);
        if (!pointerInside && currentInfoBox != null)
        {
            Destroy(currentInfoBox);
            currentInfoBox = null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // limpieza de objetos instanciados cuando este componente se apaga o destruye
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void OnDisable() => Cleanup(false);
    private void OnDestroy() => Cleanup(true);

    private void Cleanup(bool immediate)
    {
        if (!currentInfoBox) return;
        if (immediate) DestroyImmediate(currentInfoBox);
        else Destroy(currentInfoBox);
        currentInfoBox = null;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // animacion de entrada del tooltip con overshoot controlado
    ////////////////////////////////////////////////////////////////////////////////////////////
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