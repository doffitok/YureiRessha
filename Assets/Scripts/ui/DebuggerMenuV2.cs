using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

////////////////////////////////////////////////////////////////////////////////////////////
// DebuggerMenu
//
// Debugger retráctil y arrastrable.
// Permite:
// - Mostrar/ocultar su contenido (abrir/cerrar) al hacer clic en cualquier objeto asignado.
// - Arrastrar el panel libremente.
// - Controlar el número de día y el tiempo actual del DayLogic mediante un slider dinámico.
// - Editar dinero, rating base y suerte base con validación numérica.
// - Mostrar rating y suerte actuales (totales con modificadores).
// Todo configurable desde el Inspector.
////////////////////////////////////////////////////////////////////////////////////////////

[DisallowMultipleComponent]
public class DebuggerMenu : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    ////////////////////////////////////////////////////////////////////////////////////////////
    // referencias principales
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Referencias principales")]
    [SerializeField] private DayLogic dayLogic;
    [SerializeField] private GameStats gameStats;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // UI del Debugger (control de día)
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Control del día")]
    [SerializeField] private TextMeshProUGUI diaActualText;
    [SerializeField] private Button botonDiaMas;
    [SerializeField] private Button botonDiaMenos;
    [SerializeField] private Button botonReiniciarDia;
    [SerializeField] private Slider sliderTiempo;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // UI del Debugger (estadísticas)
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Campos de edición de estadísticas base")]
    [SerializeField] private TMP_InputField inputDinero;
    [SerializeField] private TMP_InputField inputRating;
    [SerializeField] private TMP_InputField inputSuerte;

    [Header("Valores actuales totales")]
    [SerializeField] private TextMeshProUGUI textoSuerteActual;
    [SerializeField] private TextMeshProUGUI textoRatingActual;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // retráctil y arrastrable
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Retráctil y arrastrable")]
    [Tooltip("Cualquier objeto clickeable que actuará como interruptor de abrir/cerrar.")]
    [SerializeField] private GameObject toggleClickTarget;

    [Tooltip("Panel del contenido del debugger (lo que se oculta/muestra)")]
    [SerializeField] private GameObject panelContenido;

    [Tooltip("Velocidad del movimiento cuando se arrastra el panel")]
    [SerializeField] private float dragSmooth = 10f;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // control interno
    ////////////////////////////////////////////////////////////////////////////////////////////
    private RectTransform rectTransform;
    private Canvas canvas;
    private bool menuVisible = true;

    private Vector2 dragOffset;
    private Vector2 targetPosition;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // inicio
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (dayLogic == null)
            dayLogic = FindFirstObjectByType<DayLogic>();
        if (gameStats == null)
            gameStats = FindFirstObjectByType<GameStats>();

        // Conectar botones
        if (botonDiaMas != null) botonDiaMas.onClick.AddListener(() => CambiarDia(1));
        if (botonDiaMenos != null) botonDiaMenos.onClick.AddListener(() => CambiarDia(-1));
        if (botonReiniciarDia != null) botonReiniciarDia.onClick.AddListener(ReiniciarDia);

        // ✅ Asignar evento de click al objeto elegido como toggle (no requiere Button)
        if (toggleClickTarget != null)
        {
            EventTrigger trigger = toggleClickTarget.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = toggleClickTarget.AddComponent<EventTrigger>();

            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener((_) => ToggleMenu());
            trigger.triggers.Add(entry);
        }

        // Slider dinámico según duración del día
        if (sliderTiempo != null && dayLogic != null)
        {
            sliderTiempo.minValue = 0;
            sliderTiempo.maxValue = dayLogic.maxSeconds;
            sliderTiempo.value = dayLogic.currentSecond;
            sliderTiempo.onValueChanged.AddListener(OnSliderTiempoCambiado);
        }

        // Inicializar inputs de stats
        if (gameStats != null)
        {
            if (inputDinero != null) inputDinero.text = gameStats.dinero.ToString();
            if (inputRating != null) inputRating.text = gameStats.rating.ToString();
            if (inputSuerte != null) inputSuerte.text = gameStats.suerte.ToString();
        }

        // Validaciones en tiempo real
        if (inputDinero != null) inputDinero.onEndEdit.AddListener(OnDineroCambiado);
        if (inputRating != null) inputRating.onEndEdit.AddListener(OnRatingCambiado);
        if (inputSuerte != null) inputSuerte.onEndEdit.AddListener(OnSuerteCambiado);

        if (panelContenido != null)
            panelContenido.SetActive(menuVisible);

        ActualizarTextoDia();
        ActualizarStatsActuales();
    }

    private void Update()
    {
        ActualizarTextoDia();
        ActualizarStatsActuales();

        if (sliderTiempo != null && dayLogic != null && dayLogic.maxSeconds > 0)
        {
            sliderTiempo.minValue = 0;
            sliderTiempo.maxValue = dayLogic.maxSeconds;
            sliderTiempo.value = dayLogic.currentSecond;
        }

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(
                rectTransform.anchoredPosition,
                targetPosition,
                Time.deltaTime * dragSmooth
            );
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // metodos de control del día
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void CambiarDia(int delta)
    {
        if (dayLogic == null) return;

        int nuevoDia = Mathf.Max(1, dayLogic.currentDay + delta);
        var field = typeof(DayLogic).GetField("<currentDay>k__BackingField",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            field.SetValue(dayLogic, nuevoDia);
            Debug.Log($"[DebuggerMenu] Día ajustado manualmente a {nuevoDia}");
        }

        ActualizarTextoDia();
    }

    private void ReiniciarDia()
    {
        if (dayLogic == null) return;

        dayLogic.ResetDay();
        Debug.Log("[DebuggerMenu] Día reiniciado manualmente desde el debugger.");

        if (sliderTiempo != null)
            sliderTiempo.value = 0;
    }

    private void OnSliderTiempoCambiado(float valor)
    {
        if (dayLogic == null) return;

        int segundos = Mathf.RoundToInt(valor);
        segundos = Mathf.Clamp(segundos, 0, Mathf.Max(0, dayLogic.maxSeconds - 2));
        dayLogic.SetCurrentSecond(segundos);
    }

    private void ActualizarTextoDia()
    {
        if (diaActualText != null && dayLogic != null)
            diaActualText.text = $"Día actual: {dayLogic.currentDay}";
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // inputs de estadísticas
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void OnDineroCambiado(string value)
    {
        if (gameStats == null || inputDinero == null) return;

        if (int.TryParse(value, out int nuevoValor))
        {
            nuevoValor = Mathf.Clamp(nuevoValor, -9999999, 9999999);
            gameStats.dinero = nuevoValor;
            inputDinero.text = nuevoValor.ToString();
        }
        else
        {
            inputDinero.text = gameStats.dinero.ToString();
        }
    }

    private void OnRatingCambiado(string value)
    {
        if (gameStats == null || inputRating == null) return;

        if (int.TryParse(value, out int nuevoValor))
        {
            nuevoValor = Mathf.Clamp(nuevoValor, 0, 100);
            gameStats.rating = nuevoValor;
            inputRating.text = nuevoValor.ToString();
        }
        else
        {
            inputRating.text = gameStats.rating.ToString();
        }
    }

    private void OnSuerteCambiado(string value)
    {
        if (gameStats == null || inputSuerte == null) return;

        if (int.TryParse(value, out int nuevoValor))
        {
            nuevoValor = Mathf.Clamp(nuevoValor, 0, 100);
            gameStats.suerte = nuevoValor;
            inputSuerte.text = nuevoValor.ToString();
        }
        else
        {
            inputSuerte.text = gameStats.suerte.ToString();
        }
    }

    private void ActualizarStatsActuales()
    {
        if (gameStats == null) return;

        if (textoSuerteActual != null)
            textoSuerteActual.text = $"Suerte total: {gameStats.GetSuerteTotal()}";

        if (textoRatingActual != null)
            textoRatingActual.text = $"Rating total: {gameStats.GetRatingTotal()}";
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // apertura / cierre del panel
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void ToggleMenu()
    {
        if (panelContenido == null) return;
        menuVisible = !menuVisible;
        panelContenido.SetActive(menuVisible);
        Debug.Log($"[DebuggerMenu] Panel {(menuVisible ? "abierto" : "cerrado")}.");
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // arrastre del panel
    ////////////////////////////////////////////////////////////////////////////////////////////
    public void OnPointerDown(PointerEventData eventData)
    {
        if (canvas == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out dragOffset
        );

        dragOffset = rectTransform.anchoredPosition - dragOffset;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out localPoint))
        {
            targetPosition = localPoint + dragOffset;
        }
    }
}