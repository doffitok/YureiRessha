using UnityEngine;
using TMPro;

public class uiDineroDisplay : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameStats gameStats;
    [SerializeField] private TextMeshProUGUI dineroDisplay;

    private int ultimoDinero = int.MinValue; // valor inicial para forzar primera actualización
    private const long LIMITE_DINERO = 2147483640; // límite para mostrar "???"

    private void Start()
    {
        // Buscar GameStats automáticamente si no se asignó
        if (gameStats == null)
            gameStats = FindObjectOfType<GameStats>();

        // Buscar el texto si no se asignó
        if (dineroDisplay == null)
        {
            Transform found = transform.Find("dineroDisplay");
            if (found != null)
                dineroDisplay = found.GetComponent<TextMeshProUGUI>();
        }

        if (gameStats == null || dineroDisplay == null)
        {
            Debug.LogError("[uiDineroDisplay] No se pudo encontrar GameStats o dineroDisplay en la escena.");
            enabled = false;
            return;
        }

        ActualizarDisplay(); // Mostrar valor inicial
    }

    private void Update()
    {
        int dineroActual = gameStats.dinero; // valor base

        // Solo actualizar si cambió
        if (dineroActual != ultimoDinero)
            ActualizarDisplay();
    }

    private void ActualizarDisplay()
    {
        long dineroActual = gameStats.dinero;

        if (dineroActual > LIMITE_DINERO)
        {
            dineroDisplay.text = "¥" + "SyntaxError";
        }
        else
        {
            dineroDisplay.text = $"¥ {dineroActual:N0}";
        }

        ultimoDinero = gameStats.dinero;
    }
}