using UnityEngine;

public class DayLogic : MonoBehaviour
{
    // Tiempo actual del día (segundos)
    public int currentSecond { get; private set; } = 0;

    [Header("Configuración del día")]
    public int maxSeconds = 300; // Duración total del día en segundos

    private bool isRunning = false; // Controla si el contador está activo
    private float timer = 0f;       // Acumula el deltaTime

    [Header("Configuración del sol (Day/Night Cycle)")]
    public Light sun; // Luz direccional (debe llamarse "sun" en la escena)
    private Color startColor = new Color(202f / 255f, 88f / 255f, 0f / 255f);   // #CA5800
    private Color endColor = new Color(30f / 255f, 79f / 255f, 78f / 255f);     // #1E4F4E
    private float startRotationX = 25f;
    private float endRotationX = 40f;

    private void Start()
    {
        StartDay(); // Inicia el día automáticamente al arrancar

        // Buscar la luz "sun" si no se asignó en el inspector
        if (sun == null)
        {
            GameObject sunObj = GameObject.Find("sun");
            if (sunObj != null)
            {
                sun = sunObj.GetComponent<Light>();
            }

            if (sun == null)
            {
                Debug.LogWarning("[DayLogic] No se encontró la luz 'sun' en la escena.");
            }
        }
    }

    private void Update()
    {
        if (isRunning)
        {
            // Acumulamos el tiempo transcurrido
            timer += Time.deltaTime;

            // Cada segundo incrementamos currentSecond
            if (timer >= 1f)
            {
                currentSecond++;
                if (currentSecond > maxSeconds)
                    currentSecond = 0; // Reinicia el contador al llegar al máximo

                timer = 0f;
            }
        }

        UpdateSunCycle();
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

    // Permite que otros scripts modifiquen currentSecond
    public void SetCurrentSecond(int value)
    {
        currentSecond = Mathf.Clamp(value, 0, maxSeconds);
    }

    // Actualiza el color y rotación de la luz
    private void UpdateSunCycle()
    {
        if (sun == null) return;

        // Normaliza el tiempo del día (0 → 1)
        float t = Mathf.Clamp01((float)currentSecond / maxSeconds);

        // Cambiar color gradualmente
        sun.color = Color.Lerp(startColor, endColor, t);

        // Cambiar rotación en X (manteniendo Y/Z iguales)
        Vector3 rot = sun.transform.rotation.eulerAngles;
        rot.x = Mathf.Lerp(startRotationX, endRotationX, t);
        sun.transform.rotation = Quaternion.Euler(rot);
    }
}