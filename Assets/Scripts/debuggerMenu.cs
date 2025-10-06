using UnityEngine;
using UnityEngine.UIElements;

public class DebuggerMenu : MonoBehaviour
{
    [Header("Referencias Opcionales")]
    public UIDocument uiDocument;

    private GameStats stats;
    private DayLogic dayLogic;
    private passengerPlacementLogic passengerLogic;

    private TextField ratingField;
    private TextField dineroField;
    private TextField suerteField;
    private Button reiniciarDiaButton;
    private Slider debuggerTiempo;

    private Button debuggerMenuButton;
    private VisualElement debuggerMenuItems;
    private bool menuVisible = false;

    private void OnEnable()
    {
        UIDocument doc = uiDocument != null ? uiDocument : GetComponent<UIDocument>();
        if (doc == null)
        {
            Debug.LogWarning("[DebuggerMenu] No se encontró UIDocument en la escena ni referencia asignada.");
            return;
        }

        var root = doc.rootVisualElement;

        // Referencias a TextFields
        ratingField = root.Q<TextField>("debuggerRating");
        dineroField = root.Q<TextField>("debuggerDinero");
        suerteField = root.Q<TextField>("debuggerSuerte");

        // Botón reiniciar día
        reiniciarDiaButton = root.Q<Button>("debuggerReiniciarDia");
        if (reiniciarDiaButton != null)
            reiniciarDiaButton.clicked += OnReiniciarDiaClicked;

        // Slider tiempo
        debuggerTiempo = root.Q<Slider>("debuggerTiempo");

        // Botón y contenedor del menú
        debuggerMenuButton = root.Q<Button>("debuggerMenu");
        debuggerMenuItems = root.Q<VisualElement>("debuggerMenuItems");
        if (debuggerMenuItems != null)
            debuggerMenuItems.style.display = DisplayStyle.None;
        if (debuggerMenuButton != null)
            debuggerMenuButton.clicked += ToggleMenu;

        // Buscar referencias en escena
        stats = FindFirstObjectByType<GameStats>();
        dayLogic = FindFirstObjectByType<DayLogic>();
        passengerLogic = FindFirstObjectByType<passengerPlacementLogic>();

        if (stats == null) Debug.LogWarning("[DebuggerMenu] No se encontró GameStats en la escena.");
        if (dayLogic == null) Debug.LogWarning("[DebuggerMenu] No se encontró DayLogic en la escena.");
        if (passengerLogic == null) Debug.LogWarning("[DebuggerMenu] No se encontró passengerPlacementLogic en la escena.");

        // Inicializar valores
        if (stats != null)
        {
            ratingField.value = stats.rating.ToString();
            dineroField.value = stats.dinero.ToString();
            suerteField.value = stats.suerte.ToString();
        }

        // Suscribir eventos usando límites fijos de GameStats
        ratingField.RegisterValueChangedCallback(evt =>
            UpdateStatValue(evt.newValue, ref stats.rating, 0, 100, ratingField));
        dineroField.RegisterValueChangedCallback(evt =>
            UpdateStatValue(evt.newValue, ref stats.dinero, 0, 10000, dineroField));
        suerteField.RegisterValueChangedCallback(evt =>
            UpdateStatValue(evt.newValue, ref stats.suerte, 0, 100, suerteField));

        // Configurar slider tiempo
        if (debuggerTiempo != null && dayLogic != null)
        {
            debuggerTiempo.lowValue = 0;
            debuggerTiempo.highValue = dayLogic.maxSeconds;
            debuggerTiempo.value = dayLogic.currentSecond;

            debuggerTiempo.RegisterValueChangedCallback(evt =>
            {
                if (dayLogic != null)
                    dayLogic.SetCurrentSecond(Mathf.Clamp(Mathf.RoundToInt(evt.newValue), 0, dayLogic.maxSeconds));
            });
        }
    }

    private void ToggleMenu()
    {
        if (debuggerMenuItems == null) return;
        menuVisible = !menuVisible;
        debuggerMenuItems.style.display = menuVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    /// <summary>
    /// Actualiza el valor de un stat usando límites fijos (no sliders de tiempo).
    /// </summary>
    private void UpdateStatValue(string input, ref int stat, int min, int max, TextField field)
    {
        if (int.TryParse(input, out int parsedValue))
        {
            parsedValue = Mathf.Clamp(parsedValue, min, max);
            stat = parsedValue;
            field.value = stat.ToString();
        }
        else
        {
            field.value = stat.ToString(); // mantener valor actual si no es válido
        }
    }

    private void OnReiniciarDiaClicked()
    {
        // Reiniciar día
        if (dayLogic != null)
        {
            dayLogic.ResetDay();
            dayLogic.StartDay();
        }

        // Regenerar pasajeros
        if (passengerLogic != null)
            passengerLogic.SpawnPassengers();

        // Actualizar slider tiempo
        if (debuggerTiempo != null && dayLogic != null)
            debuggerTiempo.value = dayLogic.currentSecond;
    }

    private void OnDisable()
    {
        if (reiniciarDiaButton != null)
            reiniciarDiaButton.clicked -= OnReiniciarDiaClicked;
        if (debuggerMenuButton != null)
            debuggerMenuButton.clicked -= ToggleMenu;
    }
}