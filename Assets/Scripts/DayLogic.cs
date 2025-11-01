using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[DisallowMultipleComponent]
public class DayLogic : MonoBehaviour
{
    // Eventos públicos
    public event System.Action OnDayStarted; // otros sistemas pueden usarlo si quieren
    public event System.Action OnDayReset;   // ← la tienda escuchará este

    // Tiempo actual del día (segundos)
    public int currentSecond { get; private set; } = 0;

    [Header("Configuración del día")]
    [Tooltip("Duración total del día en segundos")]
    public int maxSeconds = 300;

    private bool isRunning = false; // Controla si el contador está activo
    private float timer = 0f;       // Acumula el deltaTime

    [Header("Configuración del sol (Day/Night Cycle)")]
    public Light sun; // Luz direccional (debe llamarse "sun" en la escena)
    private Color startColor = new Color(202f / 255f, 88f / 255f, 0f / 255f);   // #CA5800
    private Color endColor = new Color(30f / 255f, 79f / 255f, 78f / 255f);     // #1E4F4E
    private float startRotationX = 25f;
    private float endRotationX = 40f;

    [Header("Configuración del botón de inicio (UI)")]
    [Tooltip("Botón del Canvas que inicia el día")]
    [SerializeField] private Button startButton;
    [Tooltip("RectTransform del botón (mismo objeto del botón)")]
    [SerializeField] private RectTransform buttonTransform;

    [Space(5)]
    [Tooltip("Altura del movimiento hacia arriba antes de bajar (anticipación)")]
    [SerializeField] private float anticipationHeight = 20f;

    [Tooltip("Distancia hacia abajo que se moverá el botón al presionar")]
    [SerializeField] private float slideDistance = 150f;

    [Tooltip("Velocidad de la animación (mayor = más rápido)")]
    [SerializeField] private float slideSpeed = 4f;

    [Tooltip("Duración de la anticipación (en segundos)")]
    [SerializeField] private float anticipationTime = 0.15f;

    private Vector2 originalButtonPos;
    private bool originalButtonPosCaptured = false;

    [Header("Referencias a lógica de pasajeros")]
    [SerializeField] private passengerPlacementLogic passengerPlacement;
    [SerializeField] private passengerSelectLogic passengerSelect;

    private void Start()
    {
        // Guardar la posición inicial del botón si existe
        if (buttonTransform != null && !originalButtonPosCaptured)
        {
            originalButtonPos = buttonTransform.anchoredPosition;
            originalButtonPosCaptured = true;
        }

        // Asignar listener si el botón está asignado
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonPressed);
        else
            Debug.LogWarning("[DayLogic] No hay botón asignado para iniciar el día.");

        // Buscar la luz "sun" si no se asignó en el inspector
        if (sun == null)
        {
            GameObject sunObj = GameObject.Find("sun");
            if (sunObj != null)
                sun = sunObj.GetComponent<Light>();

            if (sun == null)
                Debug.LogWarning("[DayLogic] No se encontró la luz 'sun' en la escena.");
        }

        // Estado inicial
        isRunning = false;
        timer = 0f;
        currentSecond = 0;

        // Asegurar referencias si no fueron asignadas
        if (passengerPlacement == null)
            passengerPlacement = FindFirstObjectByType<passengerPlacementLogic>();
        if (passengerSelect == null)
            passengerSelect = FindFirstObjectByType<passengerSelectLogic>();

        // Dejar botón visible y usable al comienzo
        RestoreStartButton();
        UpdateSunCycle();
    }

    private void Update()
    {
        if (isRunning)
        {
            timer += Time.deltaTime;

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

    // Inicia el día (activar el contador) — NO notifica a la tienda
    public void StartDay()
    {
        if (isRunning) return;

        isRunning = true;
        Debug.Log("[DayLogic] 🌞 El día ha comenzado.");

        // Notifica a otros sistemas (si los hay) — la TIENDA no escucha este
        OnDayStarted?.Invoke();

        // Desactivar el botón para no re-spammear
        if (startButton != null) startButton.interactable = false;

        // Lanzar selección y spawn de pasajeros de forma segura
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

    // Reinicia el día (contador a 0) y NO empieza el día — la tienda SÍ escucha esto
    public void ResetDay()
    {
        currentSecond = 0;
        timer = 0f;
        isRunning = false;

        // Notificar que el día se ha reseteado (para que la tienda regenere items)
        OnDayReset?.Invoke();

        Debug.Log("[DayLogic] 🔁 Día reseteado (sin iniciar).");
    }

    // Permite que otros scripts modifiquen currentSecond
    public void SetCurrentSecond(int value)
    {
        currentSecond = Mathf.Clamp(value, 0, maxSeconds);
    }

    // Actualiza el color y rotación de la luz del sol
    private void UpdateSunCycle()
    {
        if (sun == null) return;

        float t = Mathf.Clamp01((float)currentSecond / maxSeconds);
        sun.color = Color.Lerp(startColor, endColor, t);

        Vector3 rot = sun.transform.rotation.eulerAngles;
        rot.x = Mathf.Lerp(startRotationX, endRotationX, t);
        sun.transform.rotation = Quaternion.Euler(rot);
    }

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

        // Anticipación hacia arriba
        float t = 0f;
        float dur = Mathf.Max(anticipationTime, 0.01f);
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            buttonTransform.anchoredPosition = Vector2.Lerp(startPos, upPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        // Deslizamiento hacia abajo
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

    /// Restaura el botón de inicio a su posición y estado original.
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
}