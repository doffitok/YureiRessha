using UnityEngine;

public class DayLogic : MonoBehaviour
{
    // Tiempo actual del día (segundos)
    public int currentSecond { get; private set; } = 0;

    [Header("Configuración del día")]
    public int maxSeconds = 300; // Duración total del día en segundos

    private bool isRunning = false; // Controla si el contador está activo
    private float timer = 0f;       // Acumula el deltaTime

    private void Start()
    {
        StartDay(); // 🔹 Inicia el día automáticamente al arrancar
    }

    private void Update()
    {
        if (!isRunning) return;

        // Acumulamos el tiempo transcurrido
        timer += Time.deltaTime;

        // Cada segundo incrementamos currentSecond
        if (timer >= 1f)
        {
            currentSecond++;
            if (currentSecond > maxSeconds)
                currentSecond = 0; // 🔹 Reinicia el contador al llegar al máximo

            timer = 0f;
        }
    }

    // Inicia el día (activar el contador)
    public void StartDay()
    {
        isRunning = true;
    }

    // Reinicia el día (contador a 0) y permite volver a iniciar
    public void ResetDay()
    {
        currentSecond = 0;
        timer = 0f;
        isRunning = false; // Se puede volver a activar con StartDay()
    }
}