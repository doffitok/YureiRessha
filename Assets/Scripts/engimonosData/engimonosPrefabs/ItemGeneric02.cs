using UnityEngine;
using System.Collections;

////////////////////////////////////////////////////////////////////////////////////////////
// ITEM GENERIC 02 — GENERADOR PASIVO DE DINERO (MULTI-INSTANCIA INDEPENDIENTE)
//
// Cada copia genera dinero periodicamente de forma pasiva.
// Ademas, al activarse ejecuta una animacion de escala (agrandar -> achicar -> restaurar)
// y reproduce un sonido opcional.
//
// El sistema detecta el inicio y reinicio del dia mediante DayLogic.
// Cada Engimono tiene su propio ciclo independiente.
////////////////////////////////////////////////////////////////////////////////////////////

public class ItemGeneric02 : MonoBehaviour, IEngimonoApply
{
    ////////////////////////////////////////////////////////////////////////////////////////////
    // CONFIGURACION DEL EFECTO
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Configuracion del efecto")]
    [SerializeField] private float intervaloBase = 5f;
    [SerializeField] private float intervaloFactorMax = 2f;
    [SerializeField] private float variacionFijaMin = 0.05f;
    [SerializeField] private float variacionFijaMax = 0.25f;
    [SerializeField] private float intervaloMinimoAbsoluto = 0.1f;
    [SerializeField] private Vector2Int rangoDinero = new Vector2Int(2, 12);

    ////////////////////////////////////////////////////////////////////////////////////////////
    // SONIDO DE ACTIVACION
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Sonido")]
    [SerializeField] private AudioClip sonidoActivacion;
    [SerializeField] private float volumenMultiplicador = 1f;
    [SerializeField] private float pitchMin = 0.9f;
    [SerializeField] private float pitchMax = 1.1f;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // ANIMACION DE ESCALA (VISUAL)
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Animacion de activacion")]
    [Tooltip("Escala de la primera fase (agrandar).")]
    [SerializeField] private float escalaFase1 = 1.3f;
    [Tooltip("Escala de la segunda fase (achicar por debajo de la base).")]
    [SerializeField] private float escalaFase2 = 0.85f;
    [Tooltip("Duracion de cada fase de la animacion (en segundos).")]
    [SerializeField] private float duracionFase = 0.08f;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // REFERENCIAS OPCIONALES
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Referencias (opcional)")]
    [Tooltip("Si no se asigna, se buscara en escena automaticamente.")]
    [SerializeField] private GameStats gameStats;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // APLICA EL EFECTO AL OBJETIVO (USO INTERNO DEL SISTEMA DE ENGIMONOS)
    ////////////////////////////////////////////////////////////////////////////////////////////
    public void AplicarEfecto(GameObject _)
    {
        if (gameObject == null) return;

        var stats = gameStats != null ? gameStats : FindFirstObjectByType<GameStats>();
        if (stats == null) return;

        // Evita duplicar tickers en el mismo objeto
        var ticker = GetComponent<ItemGeneric02Ticker>();
        if (ticker == null) ticker = gameObject.AddComponent<ItemGeneric02Ticker>();

        ticker.Init(
            stats,
            intervaloBase, intervaloFactorMax,
            variacionFijaMin, variacionFijaMax, intervaloMinimoAbsoluto,
            rangoDinero,
            sonidoActivacion, volumenMultiplicador, pitchMin, pitchMax,
            escalaFase1, escalaFase2, duracionFase,
            gameObject
        );
    }
}

////////////////////////////////////////////////////////////////////////////////////////////
// ITEM GENERIC 02 TICKER — MANEJA EL CICLO DE GENERACION Y ANIMACION
//
// Este componente se anade dinamicamente a cada Engimono que use ItemGeneric02.
// Controla el temporizador, las ganancias de dinero y la animacion de escala.
// Se sincroniza con el inicio y final del dia (DayLogic).
////////////////////////////////////////////////////////////////////////////////////////////

public class ItemGeneric02Ticker : MonoBehaviour
{
    ////////////////////////////////////////////////////////////////////////////////////////////
    // REFERENCIAS PRINCIPALES
    ////////////////////////////////////////////////////////////////////////////////////////////
    private GameStats stats;
    private DayLogic dayLogic;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // PARAMETROS DE GENERACION DE DINERO
    ////////////////////////////////////////////////////////////////////////////////////////////
    private float intervaloBase;
    private float intervaloFactorMax;
    private float variacionFijaMin;
    private float variacionFijaMax;
    private float intervaloMinAbs;
    private Vector2Int rangoDinero;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // CONTROL DE TIEMPO / ESTADO
    ////////////////////////////////////////////////////////////////////////////////////////////
    private float temporizador = 0f;
    private float intervaloActual = 1f;
    private float siguienteIntervalo = 1f;
    private bool inicializado = false;
    private bool dayRunning = false;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // AUDIO
    ////////////////////////////////////////////////////////////////////////////////////////////
    private AudioClip sonido;
    private float volumenMult;
    private float pitchMin;
    private float pitchMax;
    private AudioSource audioSource;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // ANIMACION DE ESCALA
    ////////////////////////////////////////////////////////////////////////////////////////////
    private float escalaFase1;
    private float escalaFase2;
    private float duracionFase;
    private GameObject objetoVisual;
    private RectTransform rectAnimado; // Cacheado una sola vez

    ////////////////////////////////////////////////////////////////////////////////////////////
    // INICIALIZACION
    ////////////////////////////////////////////////////////////////////////////////////////////
    public void Init(
        GameStats stats,
        float intervaloBase,
        float intervaloFactorMax,
        float variacionFijaMin,
        float variacionFijaMax,
        float intervaloMinimoAbsoluto,
        Vector2Int rangoDinero,
        AudioClip sonido,
        float volumenMult,
        float pitchMin,
        float pitchMax,
        float escalaFase1,
        float escalaFase2,
        float duracionFase,
        GameObject objetoVisual
    )
    {
        // Asignacion de datos base
        this.stats = stats;
        this.intervaloBase = intervaloBase;
        this.intervaloFactorMax = intervaloFactorMax;
        this.variacionFijaMin = variacionFijaMin;
        this.variacionFijaMax = variacionFijaMax;
        this.intervaloMinAbs = intervaloMinimoAbsoluto;
        this.rangoDinero = rangoDinero;

        // Audio
        this.sonido = sonido;
        this.volumenMult = volumenMult;
        this.pitchMin = pitchMin;
        this.pitchMax = pitchMax;

        // Escala y animacion
        this.escalaFase1 = Mathf.Max(0.0001f, escalaFase1);
        this.escalaFase2 = Mathf.Max(0.0001f, escalaFase2);
        this.duracionFase = Mathf.Max(0.0001f, duracionFase);

        // Visual
        this.objetoVisual = objetoVisual != null ? objetoVisual : this.gameObject;
        rectAnimado = this.objetoVisual.GetComponent<RectTransform>();
        if (rectAnimado == null) rectAnimado = this.objetoVisual.GetComponentInChildren<RectTransform>();

        // Configuracion de audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 1f;

        // Deteccion del ciclo de dia
        dayLogic = FindFirstObjectByType<DayLogic>();
        if (dayLogic != null)
        {
            dayLogic.OnDayStarted += HandleDayStarted;
            dayLogic.OnDayReset += HandleDayReset;

            bool diaEnCurso = (dayLogic.currentSecond > 0 && dayLogic.currentSecond < dayLogic.maxSeconds);
            if (diaEnCurso)
            {
                dayRunning = true;
                StartCoroutine(ActivarTrasRetrasoAleatorio());
            }
        }
        else
        {
            StartCoroutine(RevisarInicioTardio());
        }

        inicializado = true;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // DETECCION DE DIA TARDIA
    ////////////////////////////////////////////////////////////////////////////////////////////
    private IEnumerator RevisarInicioTardio()
    {
        yield return new WaitForSeconds(0.5f);
        if (dayLogic == null) dayLogic = FindFirstObjectByType<DayLogic>();

        if (dayLogic != null)
        {
            dayLogic.OnDayStarted += HandleDayStarted;
            dayLogic.OnDayReset += HandleDayReset;

            if (dayLogic.currentSecond > 0 && dayLogic.currentSecond < dayLogic.maxSeconds)
            {
                dayRunning = true;
                StartCoroutine(ActivarTrasRetrasoAleatorio());
            }
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // LIMPIEZA DE EVENTOS
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void OnDestroy()
    {
        if (dayLogic != null)
        {
            dayLogic.OnDayStarted -= HandleDayStarted;
            dayLogic.OnDayReset -= HandleDayReset;
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // EVENTOS DE DIA (INICIO / REINICIO)
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void HandleDayStarted()
    {
        dayRunning = true;
        temporizador = 0f;
        StartCoroutine(ActivarTrasRetrasoAleatorio());
    }

    private void HandleDayReset()
    {
        dayRunning = false;
        temporizador = 0f;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // BUCLE DE ACTUALIZACION PRINCIPAL
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void Update()
    {
        if (!inicializado || !dayRunning) return;

        temporizador += Time.deltaTime;
        if (temporizador >= intervaloActual)
        {
            temporizador -= intervaloActual;
            Activar();
            intervaloActual = siguienteIntervalo;
            siguienteIntervalo = CalcularSiguienteIntervalo();
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // ARRANQUE CON RETRASO ALEATORIO (EVITA SINCRONIZAR TODAS LAS INSTANCIAS)
    ////////////////////////////////////////////////////////////////////////////////////////////
    private IEnumerator ActivarTrasRetrasoAleatorio()
    {
        yield return new WaitForSeconds(Random.Range(0.1f, 0.6f));
        intervaloActual = CalcularSiguienteIntervalo();
        siguienteIntervalo = CalcularSiguienteIntervalo();
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // ACTIVACION DEL EFECTO (GENERACION + SONIDO + ANIMACION)
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void Activar()
    {
        if (stats == null) return;

        int suerteActual = stats.GetSuerteTotal();
        int cantidad = CalcularCantidadConSuerte(rangoDinero.x, rangoDinero.y, suerteActual);
        stats.dinero += cantidad;

        // sonido
        if (sonido != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(pitchMin, pitchMax);
            audioSource.volume = Mathf.Max(0f, volumenMult);
            audioSource.PlayOneShot(sonido);
        }

        // animacion
        if (rectAnimado != null)
            StartCoroutine(AnimarEscala());
        else
            Debug.LogWarning($"[ItemGeneric02Ticker] No se encontro RectTransform para animar en '{objetoVisual.name}'.");
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // ANIMACION DE ESCALA
    ////////////////////////////////////////////////////////////////////////////////////////////
    private IEnumerator AnimarEscala()
    {
        Vector3 baseScale = rectAnimado.localScale;
        Vector3 target1 = baseScale * escalaFase1;
        Vector3 target2 = baseScale * escalaFase2;

        yield return AnimateScale(rectAnimado, baseScale, target1, duracionFase);
        yield return AnimateScale(rectAnimado, target1, target2, duracionFase);
        yield return AnimateScale(rectAnimado, target2, baseScale, duracionFase);
    }

    private IEnumerator AnimateScale(RectTransform target, Vector3 from, Vector3 to, float duration)
    {
        if (target == null || duration <= 0f) yield break;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float s = Mathf.SmoothStep(0f, 1f, t);
            target.localScale = Vector3.Lerp(from, to, s);
            yield return null;
        }
        target.localScale = to;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // CALCULOS AUXILIARES (INTERVALO Y SUERTE)
    ////////////////////////////////////////////////////////////////////////////////////////////
    private float CalcularSiguienteIntervalo()
    {
        float baseConSuerte = CalcularIntervaloConSuerte(intervaloBase, intervaloFactorMax, stats.GetSuerteTotal());
        float extra = Random.Range(variacionFijaMin, variacionFijaMax);
        bool sumar = Random.Range(1, 7) >= 4;
        return Mathf.Max(intervaloMinAbs, baseConSuerte + (sumar ? extra : -extra));
    }

    private float CalcularIntervaloConSuerte(float baseTime, float factorMax, int suerte)
    {
        suerte = Mathf.Clamp(suerte, 0, 100);
        if (suerte >= 100) return baseTime;

        float t = suerte / 100f;
        float aleatorio = Random.Range(baseTime, baseTime * factorMax);
        return Mathf.Lerp(aleatorio, baseTime, Mathf.Pow(t, 1.5f));
    }

    private int CalcularCantidadConSuerte(int min, int max, int suerte)
    {
        suerte = Mathf.Clamp(suerte, 0, 100);
        if (suerte >= 100) return max;

        int count = max - min + 1;
        float t = suerte / 100f;
        float alpha = Mathf.Lerp(0f, 4.5f, Mathf.SmoothStep(0f, 1f, t));

        const float piso = 0.02f;
        float total = 0f;
        float[] pesos = new float[count];

        for (int i = 0; i < count; i++)
        {
            float x = (i + 1) / (float)count;
            float w = Mathf.Lerp(piso, 1f, Mathf.Pow(x, alpha));
            pesos[i] = w;
            total += w;
        }

        float r = Random.value * total;
        float acum = 0f;
        for (int i = 0; i < count; i++)
        {
            acum += pesos[i];
            if (r <= acum) return min + i;
        }
        return max;
    }
}