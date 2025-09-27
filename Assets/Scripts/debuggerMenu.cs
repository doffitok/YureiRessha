using UnityEngine;
using UnityEngine.UIElements;

public class DebuggerMenu : MonoBehaviour
{
    public GameStats stats;

    private TextField ratingField;
    private TextField dineroField;
    private TextField suerteField;

    void OnEnable()
    {
        UIDocument doc = GetComponent<UIDocument>() ?? FindObjectOfType<UIDocument>();
        if (doc == null)
        {
            Debug.LogError("[DebuggerMenu] No se encontro UIDocument en la escena.");
            return;
        }

        var root = doc.rootVisualElement;
        ratingField = root.Q<TextField>("debuggerRating");
        dineroField = root.Q<TextField>("debuggerDinero");
        suerteField = root.Q<TextField>("debuggerSuerte");

        if (stats == null)
        {
            stats = FindObjectOfType<GameStats>();
            if (stats == null)
            {
                Debug.LogWarning("[DebuggerMenu] No se encontro GameStats en la escena.");
                return;
            }
        }

        // Inicializamos campos
        UpdateFieldsFromStats();

        // Suscribimos eventos de cambio
        if (ratingField != null) ratingField.RegisterValueChangedCallback(evt => OnRatingChanged(evt.newValue));
        if (dineroField != null) dineroField.RegisterValueChangedCallback(evt => OnDineroChanged(evt.newValue));
        if (suerteField != null) suerteField.RegisterValueChangedCallback(evt => OnSuerteChanged(evt.newValue));
    }

    public void UpdateFieldsFromStats()
    {
        if (stats == null) return;

        if (ratingField != null) ratingField.SetValueWithoutNotify(stats.rating.ToString());
        if (dineroField != null) dineroField.SetValueWithoutNotify(stats.dinero.ToString());
        if (suerteField != null) suerteField.SetValueWithoutNotify(stats.suerte.ToString());
    }

    private void OnRatingChanged(string input)
    {
        if (int.TryParse(input, out int value))
        {
            stats.rating = Mathf.Clamp(value, 1, 5);
            UpdateFieldsFromStats(); // Refrescar valor corregido si se paso del limite
        }
        else
        {
            UpdateFieldsFromStats(); // Revertir si el input no fue valido
        }
    }

    private void OnDineroChanged(string input)
    {
        if (int.TryParse(input, out int value))
        {
            stats.dinero = Mathf.Clamp(value, 0, 10000);
            UpdateFieldsFromStats();
        }
        else
        {
            UpdateFieldsFromStats();
        }
    }

    private void OnSuerteChanged(string input)
    {
        if (int.TryParse(input, out int value))
        {
            stats.suerte = Mathf.Clamp(value, 0, 100);
            UpdateFieldsFromStats();
        }
        else
        {
            UpdateFieldsFromStats();
        }
    }
}