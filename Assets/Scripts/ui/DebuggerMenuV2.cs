using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

////////////////////////////////////////////////////////////////////////////////////////////
// DebuggerMenu — versión final sin límite de tiempo
//
// Permite mover el slider hasta maxSeconds y mantiene el tiempo corriendo correctamente.
////////////////////////////////////////////////////////////////////////////////////////////

[DisallowMultipleComponent]
public class DebuggerMenu : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("Referencias principales")]
    [SerializeField] private DayLogic dayLogic;
    [SerializeField] private GameStats gameStats;

    [Header("Control del día")]
    [SerializeField] private TextMeshProUGUI diaActualText;
    [SerializeField] private Button botonDiaMas;
    [SerializeField] private Button botonDiaMenos;
    [SerializeField] private Button botonReiniciarDia;
    [SerializeField] private Slider sliderTiempo;

    [Header("Campos de edición de estadísticas base")]
    [SerializeField] private TMP_InputField inputDinero;
    [SerializeField] private TMP_InputField inputRating;
    [SerializeField] private TMP_InputField inputSuerte;

    [Header("Valores actuales totales")]
    [SerializeField] private TextMeshProUGUI textoSuerteActual;
    [SerializeField] private TextMeshProUGUI textoRatingActual;

    [Header("Dinero infinito")]
    [SerializeField] private Toggle toggleDineroInfinito;

    [Header("Retráctil y arrastrable")]
    [SerializeField] private GameObject toggleClickTarget;
    [SerializeField] private GameObject panelContenido;
    [SerializeField] private bool empiezaAbierto = true;
    [SerializeField] private float dragSmooth = 10f;

    private RectTransform rectTransform;
    private Canvas canvas;
    private bool menuVisible;
    private Vector2 dragOffset;
    private Vector2 targetPosition;

    private const int MAX_DINERO = 2147483640;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        targetPosition = rectTransform.anchoredPosition;

        if (dayLogic == null)
            dayLogic = FindFirstObjectByType<DayLogic>();
        if (gameStats == null)
            gameStats = FindFirstObjectByType<GameStats>();

        // Botones
        if (botonDiaMas != null) botonDiaMas.onClick.AddListener(() => CambiarDia(1));
        if (botonDiaMenos != null) botonDiaMenos.onClick.AddListener(() => CambiarDia(-1));
        if (botonReiniciarDia != null) botonReiniciarDia.onClick.AddListener(ReiniciarDia);

        // Toggle de apertura
        if (toggleClickTarget != null)
        {
            EventTrigger trigger = toggleClickTarget.GetComponent<EventTrigger>() ?? toggleClickTarget.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener((_) => ToggleMenu());
            trigger.triggers.Add(entry);
        }

        // Slider
        if (sliderTiempo != null && dayLogic != null)
        {
            sliderTiempo.minValue = 0;
            sliderTiempo.maxValue = dayLogic.maxSeconds;
            sliderTiempo.value = dayLogic.currentSecond;
            sliderTiempo.onValueChanged.AddListener(OnSliderTiempoCambiado);
        }

        // Inputs
        if (gameStats != null)
        {
            if (inputDinero != null) inputDinero.text = gameStats.dinero.ToString();
            if (inputRating != null) inputRating.text = gameStats.rating.ToString();
            if (inputSuerte != null) inputSuerte.text = gameStats.suerte.ToString();
        }

        if (inputDinero != null) inputDinero.onEndEdit.AddListener(OnDineroCambiado);
        if (inputRating != null) inputRating.onEndEdit.AddListener(OnRatingCambiado);
        if (inputSuerte != null) inputSuerte.onEndEdit.AddListener(OnSuerteCambiado);

        if (toggleDineroInfinito != null)
            toggleDineroInfinito.onValueChanged.AddListener(OnToggleDineroInfinito);

        menuVisible = empiezaAbierto;
        if (panelContenido != null)
            panelContenido.SetActive(menuVisible);

        ActualizarTextoDia();
        ActualizarStatsActuales();
    }

    private void Update()
    {
        ActualizarTextoDia();
        ActualizarStatsActuales();

        if (sliderTiempo != null && dayLogic != null)
        {
            sliderTiempo.minValue = 0;
            sliderTiempo.maxValue = dayLogic.maxSeconds;
            sliderTiempo.value = dayLogic.currentSecond;
        }

        if (toggleDineroInfinito != null && toggleDineroInfinito.isOn && gameStats != null)
        {
            gameStats.dinero = MAX_DINERO;
            if (inputDinero != null)
                inputDinero.text = MAX_DINERO.ToString();
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

    //───────────────────────────────────────────────────────────────
    // Control del día
    //───────────────────────────────────────────────────────────────
    private void CambiarDia(int delta)
    {
        if (dayLogic == null) return;

        int nuevoDia = Mathf.Max(1, dayLogic.currentDay + delta);
        var field = typeof(DayLogic).GetField("<currentDay>k__BackingField",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(dayLogic, nuevoDia);
        ActualizarTextoDia();
    }

    private void ReiniciarDia()
    {
        if (dayLogic == null) return;
        dayLogic.ResetDay();
        if (sliderTiempo != null)
            sliderTiempo.value = 0;
    }

    private void OnSliderTiempoCambiado(float valor)
    {
        if (dayLogic == null) return;

        // 🧩 Ahora puede llegar hasta maxSeconds sin restar 2
        int segundos = Mathf.RoundToInt(valor);
        segundos = Mathf.Clamp(segundos, 0, dayLogic.maxSeconds);
        dayLogic.SetCurrentSecond(segundos);

        // 🔧 Sincronizar el timer privado del DayLogic
        var timerField = typeof(DayLogic).GetField("timer",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (timerField != null)
            timerField.SetValue(dayLogic, (float)segundos);

        // Mantener el día corriendo si ya estaba en ejecución
        var isRunningField = typeof(DayLogic).GetField("isRunning",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (isRunningField != null && (bool)isRunningField.GetValue(dayLogic))
            isRunningField.SetValue(dayLogic, true);
    }

    private void ActualizarTextoDia()
    {
        if (diaActualText != null && dayLogic != null)
            diaActualText.text = $"Día actual: {dayLogic.currentDay}";
    }

    //───────────────────────────────────────────────────────────────
    // Edición de estadísticas
    //───────────────────────────────────────────────────────────────
    private void OnDineroCambiado(string value)
    {
        if (gameStats == null || inputDinero == null) return;
        if (toggleDineroInfinito != null && toggleDineroInfinito.isOn)
        {
            inputDinero.text = MAX_DINERO.ToString();
            return;
        }

        if (int.TryParse(value, out int nuevoValor))
        {
            nuevoValor = Mathf.Clamp(nuevoValor, -9999999, MAX_DINERO);
            gameStats.dinero = nuevoValor;
            inputDinero.text = nuevoValor.ToString();
        }
        else inputDinero.text = gameStats.dinero.ToString();
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
        else inputRating.text = gameStats.rating.ToString();
    }

    private void OnSuerteCambiado(string value)
    {
        if (gameStats == null || inputSuerte == null) return;
        if (int.TryParse(value, out int nuevoValor))
        {
            nuevoValor = Mathf.Clamp(nuevoValor, 0, 100);
            gameStats.suerte = nuevoValor;
            inputSuerte.text = gameStats.suerte.ToString();
        }
        else inputSuerte.text = gameStats.suerte.ToString();
    }

    private void ActualizarStatsActuales()
    {
        if (gameStats == null) return;

        if (textoSuerteActual != null)
            textoSuerteActual.text = $"Suerte total: {gameStats.GetSuerteTotal()}";
        if (textoRatingActual != null)
            textoRatingActual.text = $"Rating total: {gameStats.GetRatingTotal()}";
    }

    //───────────────────────────────────────────────────────────────
    // Dinero infinito
    //───────────────────────────────────────────────────────────────
    private void OnToggleDineroInfinito(bool activo)
    {
        if (activo && gameStats != null)
        {
            gameStats.dinero = MAX_DINERO;
            if (inputDinero != null)
                inputDinero.text = MAX_DINERO.ToString();
        }
    }

    //───────────────────────────────────────────────────────────────
    // Retráctil y arrastre
    //───────────────────────────────────────────────────────────────
    private void ToggleMenu()
    {
        if (panelContenido == null) return;
        menuVisible = !menuVisible;
        panelContenido.SetActive(menuVisible);
        Debug.Log($"[DebuggerMenu] Panel {(menuVisible ? "abierto" : "cerrado")}.");
    }

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