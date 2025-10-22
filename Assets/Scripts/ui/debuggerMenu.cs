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
    private Label suerteTotalLabel;
    private Label ratingTotalLabel;
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

        ratingField = root.Q<TextField>("debuggerRating");
        dineroField = root.Q<TextField>("debuggerDinero");
        suerteField = root.Q<TextField>("debuggerSuerte");

        suerteTotalLabel = root.Q<Label>("suerteActual");
        ratingTotalLabel = root.Q<Label>("ratingActual");

        reiniciarDiaButton = root.Q<Button>("debuggerReiniciarDia");
        if (reiniciarDiaButton != null)
            reiniciarDiaButton.clicked += OnReiniciarDiaClicked;

        debuggerTiempo = root.Q<Slider>("debuggerTiempo");

        debuggerMenuButton = root.Q<Button>("debuggerMenu");
        debuggerMenuItems = root.Q<VisualElement>("debuggerMenuItems");
        if (debuggerMenuItems != null)
            debuggerMenuItems.style.display = DisplayStyle.None;
        if (debuggerMenuButton != null)
            debuggerMenuButton.clicked += ToggleMenu;

        stats = FindFirstObjectByType<GameStats>();
        dayLogic = FindFirstObjectByType<DayLogic>();
        passengerLogic = FindFirstObjectByType<passengerPlacementLogic>();

        if (stats != null)
        {
            ratingField.value = stats.rating.ToString();
            dineroField.value = stats.dinero.ToString();
            suerteField.value = stats.suerte.ToString();
        }

        // Rating: mantener límites 0-100
        ratingField.RegisterValueChangedCallback(evt =>
        {
            UpdateStatValue(evt.newValue, ref stats.rating, 0, 100, ratingField);
            UpdateRatingLabel();
        });

        // Dinero: permitir números negativos y detectar overflow
        dineroField.RegisterValueChangedCallback(evt =>
        {
            string value = evt.newValue;

            // Permitimos temporalmente que el texto sea "-" mientras escriben
            if (value == "-") return;

            // Intentar parsear
            if (long.TryParse(value, out long parsed)) // usamos long para detectar overflow
            {
                if (parsed > int.MaxValue || parsed < int.MinValue)
                {
                    dineroField.value = "???";
                }
                else
                {
                    stats.dinero = (int)parsed;
                    dineroField.value = stats.dinero.ToString();
                }
            }
            else
            {
                // Si no es un número válido, mantener el último valor o mostrar "???"
                dineroField.value = "???";
            }
        });

        // Suerte: mantener límites 0-100
        suerteField.RegisterValueChangedCallback(evt =>
        {
            UpdateStatValue(evt.newValue, ref stats.suerte, 0, 100, suerteField);
            UpdateSuerteLabel();
        });

        // Slider tiempo
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

        UpdateSuerteLabel();
        UpdateRatingLabel();
    }

    private void Update()
    {
        UpdateSuerteLabel();
        UpdateRatingLabel();
    }

    private void ToggleMenu()
    {
        if (debuggerMenuItems == null) return;
        menuVisible = !menuVisible;
        debuggerMenuItems.style.display = menuVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }

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
            field.value = stat.ToString();
        }
    }

    private void UpdateSuerteLabel()
    {
        if (suerteTotalLabel != null && stats != null)
        {
            suerteTotalLabel.text = $"Suerte: {stats.GetSuerteTotal()}";
        }
    }

    private void UpdateRatingLabel()
    {
        if (ratingTotalLabel != null && stats != null)
        {
            ratingTotalLabel.text = $"Rating: {stats.GetRatingTotal()}";
        }
    }

    private void OnReiniciarDiaClicked()
    {
        if (dayLogic != null)
        {
            dayLogic.ResetDay();
            dayLogic.StartDay();
        }

        if (passengerLogic != null)
            passengerLogic.SpawnPassengers();

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