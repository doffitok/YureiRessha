using UnityEngine;
using UnityEngine.UIElements;

public class DebuggerMenu : MonoBehaviour
{
    [Header("Referencias Opcionales")]
    public UIDocument uiDocument; // 🔹 Si el script no está en el mismo GameObject que el UIDocument, arrastra la referencia aquí

    private GameStats stats;
    private DayLogic dayLogic;
    private PassengerPlacementLogic passengerLogic;

    private TextField ratingField;
    private TextField dineroField;
    private TextField suerteField;
    private Button reiniciarDiaButton;
    private Slider debuggerTiempo;

    // NUEVO: botón y contenedor del menú
    private Button debuggerMenuButton;
    private VisualElement debuggerMenuItems;
    private bool menuVisible = false;

    private void OnEnable()
    {
        // Obtener rootVisualElement desde UIDocument
        UIDocument doc = uiDocument != null ? uiDocument : GetComponent<UIDocument>();
        if (doc == null)
        {
            Debug.LogWarning("[DebuggerMenu] No se encontro UIDocument en la escena ni referencia asignada.");
            return;
        }
        var root = doc.rootVisualElement;

        // Referencias a los TextFields
        ratingField = root.Q<TextField>("debuggerRating");
        dineroField = root.Q<TextField>("debuggerDinero");
        suerteField = root.Q<TextField>("debuggerSuerte");

        // Botón para reiniciar el día
        reiniciarDiaButton = root.Q<Button>("debuggerReiniciarDia");
        if (reiniciarDiaButton != null)
        {
            reiniciarDiaButton.clicked += OnReiniciarDiaClicked;
        }

        // Slider para manipular el tiempo
        debuggerTiempo = root.Q<Slider>("debuggerTiempo");

        // NUEVO: botón del menú y contenedor de elementos
        debuggerMenuButton = root.Q<Button>("debuggerMenu");
        debuggerMenuItems = root.Q<VisualElement>("debuggerMenuItems");

        if (debuggerMenuItems != null)
            debuggerMenuItems.style.display = DisplayStyle.None; // oculto por default

        if (debuggerMenuButton != null)
            debuggerMenuButton.clicked += ToggleMenu;

        // Buscamos los objetos necesarios en la escena
        stats = FindFirstObjectByType<GameStats>();
        dayLogic = FindFirstObjectByType<DayLogic>();
        passengerLogic = FindFirstObjectByType<PassengerPlacementLogic>();

        if (stats == null) Debug.LogWarning("[DebuggerMenu] No se encontro GameStats en la escena.");
        if (dayLogic == null) Debug.LogWarning("[DebuggerMenu] No se encontro DayLogic en la escena.");
        if (passengerLogic == null) Debug.LogWarning("[DebuggerMenu] No se encontro PassengerPlacementLogic en la escena.");

        // Inicializamos valores en los TextFields
        if (stats != null)
        {
            ratingField.value = stats.rating.ToString();
            dineroField.value = stats.dinero.ToString();
            suerteField.value = stats.suerte.ToString();
        }

        // Suscribimos eventos de cambio de texto
        ratingField.RegisterValueChangedCallback(evt => UpdateStatValue(evt.newValue, ref stats.rating, 1, 60, ratingField));
        dineroField.RegisterValueChangedCallback(evt => UpdateStatValue(evt.newValue, ref stats.dinero, 0, 10000, dineroField));
        suerteField.RegisterValueChangedCallback(evt => UpdateStatValue(evt.newValue, ref stats.suerte, 0, 100, suerteField));

        // Configurar slider del tiempo
        if (debuggerTiempo != null && dayLogic != null)
        {
            debuggerTiempo.lowValue = 0;
            debuggerTiempo.highValue = dayLogic.maxSeconds;
            debuggerTiempo.value = dayLogic.currentSecond;

            debuggerTiempo.RegisterValueChangedCallback(evt =>
            {
                if (dayLogic != null)
                {
                    dayLogic.SetCurrentSecond(Mathf.Clamp(Mathf.RoundToInt(evt.newValue), 0, dayLogic.maxSeconds));
                }
            });
        }
    }

    private void ToggleMenu()
    {
        if (debuggerMenuItems == null) return;

        menuVisible = !menuVisible;
        debuggerMenuItems.style.display = menuVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void UpdateStatValue(string input, ref int stat, int min, int max, TextField field)
    {
        int parsedValue;
        if (int.TryParse(input, out parsedValue))
        {
            parsedValue = Mathf.Clamp(parsedValue, min, max);
            stat = parsedValue;
            field.value = stat.ToString(); // Refrescar el campo
        }
        else
        {
            field.value = stat.ToString(); // Si no es válido, mantener valor actual
        }
    }

    private void OnReiniciarDiaClicked()
    {
        // Reiniciar día y timer
        if (dayLogic != null)
        {
            dayLogic.ResetDay();
            dayLogic.StartDay(); // 🔹 Asegura que el tiempo vuelva a contar
        }

        // Regenerar pasajeros
        if (passengerLogic != null)
        {
            passengerLogic.SpawnPassengers();
        }

        // Actualizar slider también
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