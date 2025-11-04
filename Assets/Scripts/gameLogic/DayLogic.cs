using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

////////////////////////////////////////////////////////////////////////////////////////////
// DayLogic
//
// Controla el ciclo diario del juego (tiempo, reloj, sol, eventos de inicio y fin de día).
////////////////////////////////////////////////////////////////////////////////////////////

[DisallowMultipleComponent]
public class DayLogic : MonoBehaviour
{
    public event System.Action OnDayStarted;
    public event System.Action OnDayReset;
    public event System.Action OnDayEnded;

    public int currentSecond { get; private set; } = 0;
    public int currentDay { get; private set; } = 1;

    [Header("Configuración del día")]
    [Tooltip("Duración total del día en segundos")]
    public int maxSeconds = 300;

    private bool isRunning = false;
    private bool dayFinished = false;
    private float timer = 0f;

    [Header("Configuración del sol (Day/Night Cycle)")]
    public Light sun;
    private Color startColor = new Color(202f / 255f, 88f / 255f, 0f / 255f);
    private Color endColor = new Color(30f / 255f, 79f / 255f, 78f / 255f);
    private float startRotationX = 25f;
    private float endRotationX = 40f;

    [SerializeField, Tooltip("Velocidad de transición del sol (0 = instantáneo)")]
    private float sunSmoothSpeed = 2f;
    private float sunSmoothedT = 0f;

    [Header("Botón de inicio (UI)")]
    [SerializeField] private Button startButton;
    [SerializeField] private RectTransform buttonTransform;
    [Space(5)]
    [SerializeField] private float anticipationHeight = 20f;
    [SerializeField] private float slideDistance = 150f;
    [SerializeField] private float slideSpeed = 4f;
    [SerializeField] private float anticipationTime = 0.15f;
    private Vector2 originalButtonPos;
    private bool originalButtonPosCaptured = false;

    [Header("Referencias externas")]
    [SerializeField] private PassengerPlacementLogic passengerPlacement;
    [SerializeField] private PassengerSelectLogic passengerSelect;

    [Header("Configuración del reloj")]
    [SerializeField] private RectTransform relojManecilla;
    [SerializeField] private float relojRotacionCompleta = 360f;
    [SerializeField] private bool relojInvertido = false;
    private float relojAnguloInicialReal = 0f;

    [Header("Texto de día (opcional)")]
    [SerializeField] private TextMeshProUGUI textoDia;

    private void Start()
    {
        if (buttonTransform != null && !originalButtonPosCaptured)
        {
            originalButtonPos = buttonTransform.anchoredPosition;
            originalButtonPosCaptured = true;
        }

        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonPressed);
        else
            Debug.LogWarning("[DayLogic] no hay botón asignado para iniciar el día");

        if (sun == null)
        {
            GameObject sunObj = GameObject.Find("sun");
            if (sunObj != null)
                sun = sunObj.GetComponent<Light>();
        }

        if (passengerPlacement == null)
            passengerPlacement = FindFirstObjectByType<PassengerPlacementLogic>();
        if (passengerSelect == null)
            passengerSelect = FindFirstObjectByType<PassengerSelectLogic>();

        isRunning = false;
        dayFinished = false;
        timer = 0f;
        currentSecond = 0;

        if (relojManecilla != null)
            relojAnguloInicialReal = relojManecilla.localEulerAngles.z;

        RestoreStartButton();
        UpdateSunCycle(true);
        UpdateClock();
        UpdateTextoDia();
    }

    private void Update()
    {
        if (isRunning && !dayFinished)
        {
            timer += Time.deltaTime;

            if (timer >= 1f)
            {
                currentSecond++;
                timer = 0f;

                if (currentSecond >= maxSeconds)
                {
                    currentSecond = maxSeconds;
                    EndDay();
                }
            }

            UpdateSunCycle(false);
            UpdateClock();
        }
    }

    public void StartDay()
    {
        if (isRunning || dayFinished) return;

        isRunning = true;
        dayFinished = false;
        Debug.Log("[DayLogic] El día ha comenzado");
        OnDayStarted?.Invoke();

        if (startButton != null) startButton.interactable = false;

        if (passengerSelect != null)
        {
            passengerSelect.ResetSelectionState();
            passengerSelect.RunSelectionSafe();
        }
    }

    public void ResetDay()
    {
        currentSecond = 0;
        timer = 0f;
        isRunning = false;
        dayFinished = false;
        sunSmoothedT = 0f;

        OnDayReset?.Invoke();
        Debug.Log("[DayLogic] Día reseteado sin iniciar");

        UpdateSunCycle(true);
        UpdateClock();
        UpdateTextoDia();
        RestoreStartButton();
    }

    private void EndDay()
    {
        if (dayFinished) return;

        isRunning = false;
        dayFinished = true;

        OnDayEnded?.Invoke();
        Debug.Log($"[DayLogic] El día ha terminado (Día {currentDay})");

        currentDay++;
        UpdateTextoDia();
    }

    public void SetCurrentSecond(int value)
    {
        currentSecond = Mathf.Clamp(value, 0, maxSeconds);
    }

    private void UpdateSunCycle(bool instant)
    {
        if (sun == null) return;

        float targetT = Mathf.Clamp01((float)currentSecond / maxSeconds);
        sunSmoothedT = instant || sunSmoothSpeed <= 0f
            ? targetT
            : Mathf.Lerp(sunSmoothedT, targetT, Time.deltaTime * sunSmoothSpeed);

        sun.color = Color.Lerp(startColor, endColor, sunSmoothedT);

        Vector3 rot = sun.transform.rotation.eulerAngles;
        rot.x = Mathf.Lerp(startRotationX, endRotationX, sunSmoothedT);
        sun.transform.rotation = Quaternion.Euler(rot);
    }

    private void OnStartButtonPressed() => StartCoroutine(AnimateButtonAndStartDay());

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

    private void UpdateClock()
    {
        if (relojManecilla == null || maxSeconds <= 0) return;

        float progress = Mathf.Clamp01((float)currentSecond / maxSeconds);
        float angle = relojAnguloInicialReal + (relojInvertido ? -1f : 1f) * progress * relojRotacionCompleta;
        relojManecilla.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    public float DayProgress => Mathf.Clamp01((float)currentSecond / maxSeconds);

    private void UpdateTextoDia()
    {
        if (textoDia != null)
            textoDia.text = $"Día {currentDay}";
    }
}