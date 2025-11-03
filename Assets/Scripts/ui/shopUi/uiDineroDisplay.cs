using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

////////////////////////////////////////////////////////////////////////////////////////////
// interfaz del dinero
//
// este script muestra la cantidad actual de dinero en pantalla en el DineroDisplay
// genera mas textos animados cuando el dinero aumenta o disminuye
// interpola el valor mostrado, osea que hace que el numero suba o baje progresivamente en vez de solo cambiar al resultado :P
// incluye efectos de escala y fade tanto en el texto principal como para los textos de suma
////////////////////////////////////////////////////////////////////////////////////////////

[DisallowMultipleComponent]
public class uiDineroDisplay : MonoBehaviour
{
    ////////////////////////////////////////////////////////////////////////////////////////////
    // referencias principales
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Referencias principales")]
    [SerializeField] private GameStats gameStats;
    [SerializeField] private TextMeshProUGUI dineroDisplay;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // configuracion de popups de dinero
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Configuracion de popups de dinero")]
    [SerializeField] private RectTransform popupContainer;
    [SerializeField] private TextMeshProUGUI popupBaseText;
    [Tooltip("Distancia inicial desde el texto principal hacia abajo donde empiezan los popups en pixeles")]
    [SerializeField] private float popupBaseOffsetY = 50f;
    [Tooltip("Espaciado vertical entre cada popup")]
    [SerializeField] private float popupSpacing = 30f;
    [Tooltip("Duracion del fade de popups y de relajacion del texto principal en segundos")]
    [SerializeField] private float fadeTimeGlobal = 4f;
    [Tooltip("Numero maximo de popups visibles simultaneamente")]
    [SerializeField] private int popupMaxCount = 10;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // colores de los popup
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Apariencia")]
    [SerializeField] private Color colorGanar = new Color(0.1f, 1f, 0.3f);
    [SerializeField] private Color colorPerder = new Color(1f, 0.3f, 0.3f);

    ////////////////////////////////////////////////////////////////////////////////////////////
    // animacion de aparicion de los popups
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Animacion de aparicion (Pop de los logs)")]
    [SerializeField] private float popScaleStart = 0.7f;
    [SerializeField] private float popScaleBig = 1.3f;
    [SerializeField] private float popScaleSmall = 0.9f;
    [SerializeField] private float popSpeed = 8f;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // animacion del texto principal
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Animacion del texto principal")]
    [SerializeField] private float mainScaleAdd = 0.1f;
    [SerializeField] private float mainPopSpeed = 6f;
    [Tooltip("Velocidad de interpolacion del numero mostrado")]
    [SerializeField] private float numeroInterpVelocidad = 10f;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // variables internas
    ////////////////////////////////////////////////////////////////////////////////////////////
    private int ultimoDinero = int.MinValue;
    private const long LIMITE_DINERO = 2147483640;
    private List<TextMeshProUGUI> popups = new List<TextMeshProUGUI>();
    private float fadeTimer;
    private float fadeProgress = 0f;
    private Vector3 baseScale;
    private float acumuladorEscala = 0f;
    private Coroutine scaleRoutine;
    private Coroutine numeroRoutine;
    private float dineroMostrado;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // inicio del script
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void Start()
    {
        if (gameStats == null)
            gameStats = FindFirstObjectByType<GameStats>();

        if (dineroDisplay == null)
        {
            Transform found = transform.Find("dineroDisplay");
            if (found != null)
                dineroDisplay = found.GetComponent<TextMeshProUGUI>();
        }

        if (gameStats == null || dineroDisplay == null)
        {
            Debug.LogError("[uiDineroDisplay] no se pudo encontrar GameStats o dineroDisplay en la escena");
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

    ////////////////////////////////////////////////////////////////////////////////////////////
    // actualizacion por frame
    ////////////////////////////////////////////////////////////////////////////////////////////
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

            if (numeroRoutine != null)
                StopCoroutine(numeroRoutine);
            numeroRoutine = StartCoroutine(InterpolarNumero(dineroActual));

            ultimoDinero = dineroActual;
        }

        ActualizarFadeYEscala();
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // interpolacion suave del valor mostrado
    ////////////////////////////////////////////////////////////////////////////////////////////
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

    ////////////////////////////////////////////////////////////////////////////////////////////
    // actualiza el texto principal de dinero
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void ActualizarDisplay()
    {
        long dineroActual = gameStats.dinero;

        if (dineroActual > LIMITE_DINERO)
            dineroDisplay.text = "¥ SyntaxError";
        else
            dineroDisplay.text = $"¥ {dineroActual:N0}";
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // genera un popup cuando cambia el dinero
    ////////////////////////////////////////////////////////////////////////////////////////////
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

    ////////////////////////////////////////////////////////////////////////////////////////////
    // animacion del popup individual
    ////////////////////////////////////////////////////////////////////////////////////////////
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

    ////////////////////////////////////////////////////////////////////////////////////////////
    // controla el fade y la escala del texto principal y los popups
    ////////////////////////////////////////////////////////////////////////////////////////////
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

    ////////////////////////////////////////////////////////////////////////////////////////////
    // aplica un efecto de escala al texto principal segun el cambio de dinero
    ////////////////////////////////////////////////////////////////////////////////////////////
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

    ////////////////////////////////////////////////////////////////////////////////////////////
    // animacion de escala del texto principal
    ////////////////////////////////////////////////////////////////////////////////////////////
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