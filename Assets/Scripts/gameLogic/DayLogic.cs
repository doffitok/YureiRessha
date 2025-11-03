using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

////////////////////////////////////////////////////////////////////////////////////////////
// logica del dia
//
// este script controla el ciclo diario del juego incluyendo tiempo, sol, reloj y boton de inicio
// mantiene el tiempo en segundos y ejecuta eventos cuando el dia comienza o se reinicia
// maneja el color y rotacion del sol segun la hora del dia
// controla la animacion del boton de inicio y el movimiento de la manecilla del reloj
// actualiza la progresion del dia y expone un valor publico de progreso
////////////////////////////////////////////////////////////////////////////////////////////

[DisallowMultipleComponent]
public class DayLogic : MonoBehaviour
{
    ////////////////////////////////////////////////////////////////////////////////////////////
    // eventos y variables basicas
    ////////////////////////////////////////////////////////////////////////////////////////////
    public event System.Action OnDayStarted;
    public event System.Action OnDayReset;
    public event System.Action OnDayEnded;

    public int currentSecond { get; private set; } = 0;
    public int currentDay { get; private set; } = 1;

    [Header("Configuracion del dia")]
    [Tooltip("Duracion total del dia en segundos")]
    public int maxSeconds = 300;

    private bool isRunning = false;
    private bool dayFinished = false;
    private float timer = 0f;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // configuracion del sol (ciclo dia noche)
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Configuracion del sol (Day/Night Cycle)")]
    public Light sun;
    private Color startColor = new Color(202f / 255f, 88f / 255f, 0f / 255f);
    private Color endColor = new Color(30f / 255f, 79f / 255f, 78f / 255f);
    private float startRotationX = 25f;
    private float endRotationX = 40f;

    [Tooltip("Velocidad de transicion del sol (0 = instantaneo)")]
    [SerializeField] private float sunSmoothSpeed = 2f;

    private float sunSmoothedT = 0f;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // boton de inicio
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Configuracion del boton de inicio (UI)")]
    [SerializeField] private Button startButton;
    [SerializeField] private RectTransform buttonTransform;

    [Space(5)]
    [SerializeField] private float anticipationHeight = 20f;
    [SerializeField] private float slideDistance = 150f;
    [SerializeField] private float slideSpeed = 4f;
    [SerializeField] private float anticipationTime = 0.15f;

    private Vector2 originalButtonPos;
    private bool originalButtonPosCaptured = false;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // pasajeros (referencias externas)
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Referencias a logica de pasajeros")]
    [SerializeField] private PassengerPlacementLogic passengerPlacement;
    [SerializeField] private PassengerSelectLogic passengerSelect;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // reloj (manecilla ui)
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Configuracion del reloj")]
    [Tooltip("RectTransform de la manecilla del reloj (UI Image)")]
    [SerializeField] private RectTransform relojManecilla;
    [Tooltip("Rotacion total en grados que recorre en un dia completo")]
    [SerializeField] private float relojRotacionCompleta = 360f;
    [Tooltip("Invertir sentido de rotacion")]
    [SerializeField] private bool relojInvertido = false;
    private float relojAnguloInicialReal = 0f;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // texto del día
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Texto de día (opcional)")]
    [Tooltip("Texto que mostrará el número de día en formato 'Día X'")]
    [SerializeField] private TextMeshProUGUI textoDia;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // inicio del script
    ////////////////////////////////////////////////////////////////////////////////////////////
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
            Debug.LogWarning("[DayLogic] no hay boton asignado para iniciar el dia");

        if (sun == null)
        {
            GameObject sunObj = GameObject.Find("sun");
            if (sunObj != null)
                sun = sunObj.GetComponent<Light>();

            if (sun == null)
                Debug.LogWarning("[DayLogic] no se encontro la luz sun en la escena");
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

    ////////////////////////////////////////////////////////////////////////////////////////////
    // actualizacion por frame
    ////////////////////////////////////////////////////////////////////////////////////////////
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

    ////////////////////////////////////////////////////////////////////////////////////////////
    // metodos principales del dia
    ////////////////////////////////////////////////////////////////////////////////////////////
    public void StartDay()
    {
        if (isRunning || dayFinished) return;

        isRunning = true;
        dayFinished = false;
        Debug.Log("[DayLogic] el dia ha comenzado");
        OnDayStarted?.Invoke();

        if (startButton != null) startButton.interactable = false;

        if (passengerSelect != null)
        {
            passengerSelect.ResetSelectionState();
            passengerSelect.RunSelectionSafe();
        }
        else
        {
            Debug.LogWarning("[DayLogic] no hay referencia a PassengerSelectLogic");
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
        Debug.Log("[DayLogic] dia reseteado sin iniciar");

        UpdateSunCycle(true);
        UpdateClock();
        UpdateTextoDia();

        // 🔹 restaurar botón de inicio
        RestoreStartButton();

        // 🔹 eliminar pasajeros de la escena
        if (passengerPlacement != null)
        {
            foreach (Transform child in passengerPlacement.transform)
                GameObject.Destroy(child.gameObject);
        }

        // 🔹 resetear selección de pasajeros
        if (passengerSelect != null)
            passengerSelect.ResetSelectionState();
    }

    private void EndDay()
    {
        isRunning = false;
        dayFinished = true;
        currentDay++;
        OnDayEnded?.Invoke();
        Debug.Log("[DayLogic] el dia ha terminado (Día " + currentDay + ")");
        UpdateTextoDia();
    }

    public void SetCurrentSecond(int value)
    {
        currentSecond = Mathf.Clamp(value, 0, maxSeconds);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // ciclo solar con suavizado de color y rotacion
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void UpdateSunCycle(bool instant)
    {
        if (sun == null || dayFinished) return;

        float targetT = Mathf.Clamp01((float)currentSecond / maxSeconds);

        if (instant || sunSmoothSpeed <= 0f)
            sunSmoothedT = targetT;
        else
            sunSmoothedT = Mathf.Lerp(sunSmoothedT, targetT, Time.deltaTime * sunSmoothSpeed);

        sun.color = Color.Lerp(startColor, endColor, sunSmoothedT);

        Vector3 rot = sun.transform.rotation.eulerAngles;
        rot.x = Mathf.Lerp(startRotationX, endRotationX, sunSmoothedT);
        sun.transform.rotation = Quaternion.Euler(rot);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // animacion del boton de inicio
    ////////////////////////////////////////////////////////////////////////////////////////////
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

    ////////////////////////////////////////////////////////////////////////////////////////////
    // reloj de interfaz (rotacion de la manecilla)
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void UpdateClock()
    {
        if (relojManecilla == null || maxSeconds <= 0) return;

        float progress = Mathf.Clamp01((float)currentSecond / maxSeconds);
        float angle = relojAnguloInicialReal + (relojInvertido ? -1f : 1f) * progress * relojRotacionCompleta;

        if (angle > 360f) angle = 360f;
        if (angle < -360f) angle = -360f;

        relojManecilla.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // utilidad publica de progreso del dia
    ////////////////////////////////////////////////////////////////////////////////////////////
    public float DayProgress => Mathf.Clamp01((float)currentSecond / maxSeconds);

    ////////////////////////////////////////////////////////////////////////////////////////////
    // actualizacion del texto de día
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void UpdateTextoDia()
    {
        if (textoDia != null)
            textoDia.text = $"Día {currentDay}";
    }
}