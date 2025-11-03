using UnityEngine;
using UnityEngine.UIElements;

public class DebuggerMenu : MonoBehaviour
{
    [Header("Referencias Opcionales")]
    public UIDocument uiDocument;

    private GameStats stats;
    private DayLogic dayLogic;
    private PassengerPlacementLogic passengerLogic;
    private PassengerSelectLogic passengerSelect;

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
        passengerLogic = FindFirstObjectByType<PassengerPlacementLogic>();
        passengerSelect = FindFirstObjectByType<PassengerSelectLogic>();

        if (stats != null)
        {
            ratingField.value = stats.rating.ToString();
            dineroField.value = stats.dinero.ToString();
            suerteField.value = stats.suerte.ToString();
        }

        // Rating: mantener límites 0-100
        ratingField?.RegisterValueChangedCallback(evt =>
        {
            UpdateStatValue(evt.newValue, ref stats.rating, 0, 100, ratingField);
            UpdateRatingLabel();
        });

        // Dinero: permitir negativos y controlar overflow
        dineroField?.RegisterValueChangedCallback(evt =>
        {
            string value = evt.newValue;
            if (value == "-") return;

            if (long.TryParse(value, out long parsed))
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
                dineroField.value = "???";
            }
        });

        // Suerte: mantener límites 0-100
        suerteField?.RegisterValueChangedCallback(evt =>
        {
            UpdateStatValue(evt.newValue, ref stats.suerte, 0, 100, suerteField);
            UpdateSuerteLabel();
        });

        // Slider tiempo (control manual)
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
            suerteTotalLabel.text = $"Suerte: {stats.GetSuerteTotal()}";
    }

    private void UpdateRatingLabel()
    {
        if (ratingTotalLabel != null && stats != null)
            ratingTotalLabel.text = $"Rating: {stats.GetRatingTotal()}";
    }

    // 🔁 Reiniciar día SIN empezarlo y restaurando UI / limpiando escena
    private void OnReiniciarDiaClicked()
    {
        if (dayLogic != null)
        {
            // Reinicia el ciclo (sin arrancarlo)
            dayLogic.ResetDay();
            dayLogic.SetCurrentSecond(0);

            // 🔹 Restablece el sol visualmente a su estado inicial
            var sun = dayLogic.GetType().GetField("sun",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.GetValue(dayLogic) as Light;

            if (sun != null)
            {
                Vector3 rot = sun.transform.rotation.eulerAngles;
                rot.x = 25f; // mismo valor de startRotationX
                sun.transform.rotation = Quaternion.Euler(rot);
                sun.color = new Color(202f / 255f, 88f / 255f, 0f / 255f); // color inicial
            }

            // 🔹 Restaura el botón de inicio a su posición original
            dayLogic.RestoreStartButton();
        }

        // 🔹 Borra todos los pasajeros del día anterior
        if (passengerLogic != null)
        {
            foreach (Transform child in passengerLogic.transform)
                GameObject.Destroy(child.gameObject);
        }

        // 🔹 Permite que la selección se pueda volver a ejecutar
        if (passengerSelect == null)
            passengerSelect = FindFirstObjectByType<PassengerSelectLogic>();
        if (passengerSelect != null)
            passengerSelect.ResetSelectionState();

        // 🔹 Reset visual del slider
        if (debuggerTiempo != null)
            debuggerTiempo.value = 0;

        Debug.Log("[DebuggerMenu] 🔄 Día reiniciado correctamente: tiempo y sol reseteados, pasajeros limpiados, botón restaurado.");
    }

    private void OnDisable()
    {
        if (reiniciarDiaButton != null)
            reiniciarDiaButton.clicked -= OnReiniciarDiaClicked;
        if (debuggerMenuButton != null)
            debuggerMenuButton.clicked -= ToggleMenu;
    }
}