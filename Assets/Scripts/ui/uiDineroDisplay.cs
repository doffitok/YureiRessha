using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class uiDineroDisplay : MonoBehaviour
{
    [Header("Referencias principales")]
    [SerializeField] private GameStats gameStats;
    [SerializeField] private TextMeshProUGUI dineroDisplay;

    [Header("Configuración de popups de dinero")]
    [SerializeField] private RectTransform popupContainer;
    [SerializeField] private TextMeshProUGUI popupBaseText;
    [Tooltip("Distancia inicial desde el texto principal hacia abajo donde empiezan los popups (en píxeles).")]
    [SerializeField] private float popupBaseOffsetY = 50f;
    [Tooltip("Espaciado vertical entre cada popup.")]
    [SerializeField] private float popupSpacing = 30f;
    [Tooltip("Duración del fade de popups y de relajación del texto principal (en segundos).")]
    [SerializeField] private float fadeTimeGlobal = 4f;
    [Tooltip("Número máximo de popups visibles simultáneamente.")]
    [SerializeField] private int popupMaxCount = 10;

    [Header("Apariencia")]
    [SerializeField] private Color colorGanar = new Color(0.1f, 1f, 0.3f);
    [SerializeField] private Color colorPerder = new Color(1f, 0.3f, 0.3f);

    [Header("Animación de aparición (Pop de los logs)")]
    [SerializeField] private float popScaleStart = 0.7f;
    [SerializeField] private float popScaleBig = 1.3f;
    [SerializeField] private float popScaleSmall = 0.9f;
    [SerializeField] private float popSpeed = 8f;

    [Header("Animación del texto principal")]
    [SerializeField] private float mainScaleAdd = 0.1f;
    [SerializeField] private float mainPopSpeed = 6f;
    [Tooltip("Velocidad de interpolación del número mostrado.")]
    [SerializeField] private float numeroInterpVelocidad = 10f;

    // Internos
    private int ultimoDinero = int.MinValue;
    private const long LIMITE_DINERO = 2147483640;

    private List<TextMeshProUGUI> popups = new List<TextMeshProUGUI>();
    private float fadeTimer;
    private float fadeProgress = 0f;

    private Vector3 baseScale;
    private float acumuladorEscala = 0f;
    private Coroutine scaleRoutine;
    private Coroutine numeroRoutine;
    private float dineroMostrado; // dinero interpolado suavemente

    private void Start()
    {
        if (gameStats == null)
            gameStats = FindObjectOfType<GameStats>();

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

        if (popupContainer == null)
            popupContainer = transform as RectTransform;

        if (popupBaseText != null)
            popupBaseText.gameObject.SetActive(false);

        fadeTimer = fadeTimeGlobal;
        baseScale = dineroDisplay.rectTransform.localScale;
        dineroMostrado = gameStats.dinero;
        ActualizarDisplay();
    }

    private void Update()
    {
        int dineroActual = gameStats.dinero;

        if (dineroActual != ultimoDinero)
        {
            int diferencia = dineroActual - ultimoDinero;
            if (ultimoDinero != int.MinValue && diferencia != 0)
            {
                SpawnPopup(diferencia);
                TriggerMainTextPop(diferencia);
            }

            // Iniciar interpolación suave del dinero
            if (numeroRoutine != null)
                StopCoroutine(numeroRoutine);
            numeroRoutine = StartCoroutine(InterpolarNumero(dineroActual));

            ultimoDinero = dineroActual;
        }

        ActualizarFadeYEscala();
    }

    private IEnumerator InterpolarNumero(int dineroFinal)
    {
        while (Mathf.Abs(dineroMostrado - dineroFinal) > 0.01f)
        {
            dineroMostrado = Mathf.Lerp(dineroMostrado, dineroFinal, Time.deltaTime * numeroInterpVelocidad);

            long dineroInt = Mathf.RoundToInt(dineroMostrado);
            if (dineroInt > LIMITE_DINERO)
                dineroDisplay.text = "¥ SyntaxError";
            else
                dineroDisplay.text = $"¥ {dineroInt:N0}";

            yield return null;
        }

        dineroDisplay.text = $"¥ {dineroFinal:N0}";
        dineroMostrado = dineroFinal;
    }

    private void ActualizarDisplay()
    {
        long dineroActual = gameStats.dinero;

        if (dineroActual > LIMITE_DINERO)
            dineroDisplay.text = "¥ SyntaxError";
        else
            dineroDisplay.text = $"¥ {dineroActual:N0}";
    }

    private void SpawnPopup(int diferencia)
    {
        if (popupBaseText == null || popupContainer == null) return;

        TextMeshProUGUI popup = Instantiate(popupBaseText, popupContainer);
        popup.gameObject.SetActive(true);

        string signo = diferencia > 0 ? "+" : "-";
        popup.text = $"{signo}¥{Mathf.Abs(diferencia):N0}";
        popup.color = diferencia > 0 ? colorGanar : colorPerder;
        popup.alpha = 1f;

        RectTransform rt = popup.rectTransform;
        rt.localScale = Vector3.one * popScaleStart;

        popups.Insert(0, popup);
        if (popups.Count > popupMaxCount)
        {
            Destroy(popups[popups.Count - 1].gameObject);
            popups.RemoveAt(popups.Count - 1);
        }

        for (int i = 0; i < popups.Count; i++)
        {
            RectTransform r = popups[i].rectTransform;
            r.anchoredPosition = new Vector2(0f, -(popupBaseOffsetY + popupSpacing * i));
        }

        fadeTimer = fadeTimeGlobal;
        fadeProgress = 0f;

        foreach (var p in popups)
            if (p != null) p.alpha = 1f;

        StartCoroutine(AnimarPopup(rt));
    }

    private IEnumerator AnimarPopup(RectTransform rt)
    {
        if (rt == null) yield break;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * popSpeed;
            float s = Mathf.SmoothStep(popScaleStart, popScaleBig, t);
            rt.localScale = Vector3.one * s;
            yield return null;
        }
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * popSpeed;
            float s = Mathf.SmoothStep(popScaleBig, popScaleSmall, t);
            rt.localScale = Vector3.one * s;
            yield return null;
        }
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * popSpeed;
            float s = Mathf.SmoothStep(popScaleSmall, 1f, t);
            rt.localScale = Vector3.one * s;
            yield return null;
        }

        rt.localScale = Vector3.one;
    }

    private void ActualizarFadeYEscala()
    {
        if (popups.Count > 0)
            fadeTimer -= Time.deltaTime;

        fadeProgress = 1f - Mathf.Clamp01(fadeTimer / fadeTimeGlobal);
        float alpha = Mathf.Lerp(1f, 0f, fadeProgress);

        foreach (var popup in popups)
            if (popup != null)
                popup.alpha = alpha;

        if (fadeTimer <= 0f)
        {
            foreach (var popup in popups)
                if (popup != null) Destroy(popup.gameObject);
            popups.Clear();
            fadeTimer = 0f;
            acumuladorEscala = 0f;
            dineroDisplay.rectTransform.localScale = baseScale;
        }

        if (fadeTimer < fadeTimeGlobal && acumuladorEscala > 0f)
        {
            float f = 1f - Mathf.Clamp01(fadeTimer / fadeTimeGlobal);
            float suavizado = Mathf.Lerp(1f + acumuladorEscala, 1f, f);
            dineroDisplay.rectTransform.localScale = baseScale * suavizado;
        }
    }

    private void TriggerMainTextPop(int diferencia)
    {
        if (diferencia > 0)
        {
            acumuladorEscala += mainScaleAdd;
            fadeTimer = fadeTimeGlobal;
        }
        else
        {
            fadeTimer = fadeTimeGlobal;
        }

        if (scaleRoutine != null)
            StopCoroutine(scaleRoutine);

        scaleRoutine = StartCoroutine(MainTextPopAnimation());
    }

    private IEnumerator MainTextPopAnimation()
    {
        RectTransform rt = dineroDisplay.rectTransform;
        Vector3 objetivo = baseScale * (1f + acumuladorEscala);

        float t = 0f;
        Vector3 inicio = rt.localScale;
        while (t < 1f)
        {
            t += Time.deltaTime * mainPopSpeed;
            float s = Mathf.SmoothStep(0f, 1f, t);
            rt.localScale = Vector3.Lerp(inicio, objetivo, s);
            yield return null;
        }

        rt.localScale = objetivo;
    }
}