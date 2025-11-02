using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[DisallowMultipleComponent]
public class DayLogic : MonoBehaviour
{
    // ============================================================================
    // EVENTOS Y VARIABLES BÁSICAS
    // ============================================================================
    public event System.Action OnDayStarted;
    public event System.Action OnDayReset;

    public int currentSecond { get; private set; } = 0;

    [Header("Configuración del día")]
    [Tooltip("Duración total del día en segundos")]
    public int maxSeconds = 300;

    private bool isRunning = false;
    private float timer = 0f;

    // ============================================================================
    // CONFIGURACIÓN DEL SOL (CICLO DÍA/NOCHE)
    // ============================================================================
    [Header("Configuración del sol (Day/Night Cycle)")]
    public Light sun;
    private Color startColor = new Color(202f / 255f, 88f / 255f, 0f / 255f);   // #CA5800
    private Color endColor   = new Color(30f / 255f, 79f / 255f, 78f / 255f);   // #1E4F4E
    private float startRotationX = 25f;
    private float endRotationX   = 40f;

    [Tooltip("Velocidad de transición del sol (0 = instantáneo)")]
    [SerializeField] private float sunSmoothSpeed = 2f;

    // Para suavizar transición
    private float sunSmoothedT = 0f;

    // ============================================================================
    // BOTÓN DE INICIO
    // ============================================================================
    [Header("Configuración del botón de inicio (UI)")]
    [SerializeField] private Button startButton;
    [SerializeField] private RectTransform buttonTransform;

    [Space(5)]
    [SerializeField] private float anticipationHeight = 20f;
    [SerializeField] private float slideDistance = 150f;
    [SerializeField] private float slideSpeed = 4f;
    [SerializeField] private float anticipationTime = 0.15f;

    private Vector2 originalButtonPos;
    private bool originalButtonPosCaptured = false;

    // ============================================================================
    // PASAJEROS (referencias externas)
    // ============================================================================
    [Header("Referencias a lógica de pasajeros")]
    [SerializeField] private passengerPlacementLogic passengerPlacement;
    [SerializeField] private passengerSelectLogic passengerSelect;

    // ============================================================================
    // RELOJ (manecilla UI)
    // ============================================================================
    [Header("Configuración del reloj")]
    [Tooltip("RectTransform de la manecilla del reloj (UI Image)")]
    [SerializeField] private RectTransform relojManecilla;

    [Tooltip("Rotación total en grados que recorre en un día completo")]
    [SerializeField] private float relojRotacionCompleta = 360f;

    [Tooltip("Invertir sentido de rotación")]
    [SerializeField] private bool relojInvertido = false;

    private float relojAnguloInicialReal = 0f;

    // ============================================================================
    // INICIO
    // ============================================================================
    private void Start()
    {
        // Guardar posición original del botón
        if (buttonTransform != null && !originalButtonPosCaptured)
        {
            originalButtonPos = buttonTransform.anchoredPosition;
            originalButtonPosCaptured = true;
        }

        // Asignar listener
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonPressed);
        else
            Debug.LogWarning("[DayLogic] No hay botón asignado para iniciar el día.");

        // Buscar luz
        if (sun == null)
        {
            GameObject sunObj = GameObject.Find("sun");
            if (sunObj != null)
                sun = sunObj.GetComponent<Light>();

            if (sun == null)
                Debug.LogWarning("[DayLogic] No se encontró la luz 'sun' en la escena.");
        }

        // Asegurar referencias
        if (passengerPlacement == null)
            passengerPlacement = FindFirstObjectByType<passengerPlacementLogic>();
        if (passengerSelect == null)
            passengerSelect = FindFirstObjectByType<passengerSelectLogic>();

        // Estado inicial
        isRunning = false;
        timer = 0f;
        currentSecond = 0;

        // Capturar el ángulo inicial real del reloj
        if (relojManecilla != null)
            relojAnguloInicialReal = relojManecilla.localEulerAngles.z;

        RestoreStartButton();
        UpdateSunCycle(true);
        UpdateClock();
    }

    // ============================================================================
    // UPDATE
    // ============================================================================
    private void Update()
    {
        if (isRunning)
        {
            timer += Time.deltaTime;

            if (timer >= 1f)
            {
                currentSecond++;
                if (currentSecond > maxSeconds)
                    currentSecond = 0;

                timer = 0f;
            }
        }

        // ☀️ Sol fluido (usa Time.deltaTime para suavizar)
        UpdateSunCycle(false);

        // 🕒 Reloj a saltos
        UpdateClock();
    }

    // ============================================================================
    // MÉTODOS PRINCIPALES DE DÍA
    // ============================================================================
    public void StartDay()
    {
        if (isRunning) return;

        isRunning = true;
        Debug.Log("[DayLogic] 🌞 El día ha comenzado.");
        OnDayStarted?.Invoke();

        if (startButton != null) startButton.interactable = false;

        if (passengerSelect != null)
        {
            passengerSelect.ResetSelectionState();
            passengerSelect.RunSelectionSafe();
        }
        else
        {
            Debug.LogWarning("[DayLogic] No hay referencia a passengerSelectLogic.");
        }
    }

    public void ResetDay()
    {
        currentSecond = 0;
        timer = 0f;
        isRunning = false;
        sunSmoothedT = 0f;

        OnDayReset?.Invoke();
        Debug.Log("[DayLogic] 🔁 Día reseteado (sin iniciar).");

        UpdateSunCycle(true);
        UpdateClock();
    }

    public void SetCurrentSecond(int value)
    {
        currentSecond = Mathf.Clamp(value, 0, maxSeconds);
    }

    // ============================================================================
    // CICLO SOLAR (ahora con suavizado)
    // ============================================================================
    private void UpdateSunCycle(bool instant)
    {
        if (sun == null) return;

        float targetT = Mathf.Clamp01((float)currentSecond / maxSeconds);

        // Suavizado entre frames
        if (instant || sunSmoothSpeed <= 0f)
            sunSmoothedT = targetT;
        else
            sunSmoothedT = Mathf.Lerp(sunSmoothedT, targetT, Time.deltaTime * sunSmoothSpeed);

        sun.color = Color.Lerp(startColor, endColor, sunSmoothedT);

        Vector3 rot = sun.transform.rotation.eulerAngles;
        rot.x = Mathf.Lerp(startRotationX, endRotationX, sunSmoothedT);
        sun.transform.rotation = Quaternion.Euler(rot);
    }

    // ============================================================================
    // ANIMACIÓN DEL BOTÓN
    // ============================================================================
    private void OnStartButtonPressed()
    {
        StartCoroutine(AnimateButtonAndStartDay());
    }

    private IEnumerator AnimateButtonAndStartDay()
    {
        if (buttonTransform == null)
        {
            StartDay();
            yield break;
        }

        if (startButton != null) startButton.interactable = false;

        Vector2 startPos = buttonTransform.anchoredPosition;
        if (!originalButtonPosCaptured)
        {
            originalButtonPos = startPos;
            originalButtonPosCaptured = true;
        }

        Vector2 upPos = startPos + Vector2.up * anticipationHeight;
        Vector2 downPos = startPos - Vector2.up * slideDistance;

        float t = 0f;
        float dur = Mathf.Max(anticipationTime, 0.01f);
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            buttonTransform.anchoredPosition = Vector2.Lerp(startPos, upPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        t = 0f;
        float speed = Mathf.Max(slideSpeed, 0.01f);
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            buttonTransform.anchoredPosition = Vector2.Lerp(upPos, downPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        StartDay();
    }

    public void RestoreStartButton()
    {
        if (buttonTransform != null && originalButtonPosCaptured)
            buttonTransform.anchoredPosition = originalButtonPos;

        if (startButton != null)
        {
            startButton.gameObject.SetActive(true);
            startButton.interactable = true;
        }
    }

    // ============================================================================
    // RELOJ (rotación de la manecilla)
    // ============================================================================
    private void UpdateClock()
    {
        if (relojManecilla == null || maxSeconds <= 0) return;

        float progress = Mathf.Clamp01((float)currentSecond / maxSeconds);
        float angle = relojAnguloInicialReal + (relojInvertido ? -1f : 1f) * progress * relojRotacionCompleta;

        relojManecilla.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    // ============================================================================
    // UTILIDAD PÚBLICA
    // ============================================================================
    public float DayProgress => Mathf.Clamp01((float)currentSecond / maxSeconds);
}