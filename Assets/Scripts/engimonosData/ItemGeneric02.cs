using UnityEngine;

//
// ItemGeneric02.cs — dinero pasivo + sonido por activación
// - Cada copia añade su propio ticker independiente.
// - SOLO se activa cuando el día está corriendo (DayLogic).
// - Usa eventos OnDayStarted / OnDayReset para pausar/reanudar.
// - Reproduce un AudioClip con volumen multiplicador y pitch aleatorio por activación.
//

public class ItemGeneric02 : MonoBehaviour, IEngimonoApply
{
    [Header("Configuración del efecto")]
    [Tooltip("Tiempo entre cada entrega de dinero (en segundos).")]
    [SerializeField] private float intervalo = 5f;

    [Tooltip("Rango de dinero aleatorio que se entrega cada vez (incluye extremos).")]
    [SerializeField] private Vector2Int rangoDinero = new Vector2Int(2, 12);

    [Header("Configuración de sonido")]
    [Tooltip("Sonido que se reproducirá en cada activación (opcional).")]
    [SerializeField] private AudioClip sonidoActivacion;

    [Tooltip("Multiplicador de volumen (1 = normal, >1 más fuerte, <1 más suave).")]
    [SerializeField] private float volumenMultiplicador = 1f;

    [Tooltip("Pitch mínimo posible por activación.")]
    [SerializeField] private float pitchMin = 0.9f;

    [Tooltip("Pitch máximo posible por activación.")]
    [SerializeField] private float pitchMax = 1.1f;

    public void AplicarEfecto(GameObject objetivo)
    {
        if (objetivo == null)
        {
            Debug.LogWarning("[ItemGeneric02] Objetivo nulo al aplicar efecto.");
            return;
        }

        var stats = objetivo.GetComponent<GameStats>();
        if (stats == null)
        {
            Debug.LogWarning("[ItemGeneric02] No se encontró GameStats en el objetivo.");
            return;
        }

        var ticker = objetivo.AddComponent<ItemGeneric02Ticker>();
        ticker.Init(stats, intervalo, rangoDinero, sonidoActivacion, volumenMultiplicador, pitchMin, pitchMax);

        Debug.Log($"[ItemGeneric02] Generador creado: cada {intervalo:0.##}s dará ¥{rangoDinero.x}–¥{rangoDinero.y}.");
    }
}

public class ItemGeneric02Ticker : MonoBehaviour
{
    // Lógica de dinero
    private GameStats stats;
    private float intervalo = 5f;
    private Vector2Int rangoDinero = new Vector2Int(2, 12);
    private float temporizador = 0f;
    private bool inicializado = false;

    // Estado del día
    private DayLogic dayLogic;
    private bool dayRunning = false;  // true = el día está corriendo

    // Sonido
    private AudioClip sonido;
    private float volumenMult = 1f;
    private float pitchMin = 1f;
    private float pitchMax = 1f;
    private AudioSource audioSource;

    /// <summary>
    /// Inicializa inmediatamente después de AddComponent.
    /// </summary>
    public void Init(GameStats stats, float intervalo, Vector2Int rango, AudioClip sonido, float volMult, float pitchMin, float pitchMax)
    {
        this.stats = stats;
        this.intervalo = Mathf.Max(0.1f, intervalo);
        this.rangoDinero = new Vector2Int(Mathf.Min(rango.x, rango.y), Mathf.Max(rango.x, rango.y));

        this.sonido = sonido;
        this.volumenMult = volMult;
        this.pitchMin = pitchMin;
        this.pitchMax = pitchMax;

        // Buscar DayLogic una vez y suscribirse a eventos
        dayLogic = FindFirstObjectByType<DayLogic>();
        if (dayLogic != null)
        {
            dayLogic.OnDayStarted += HandleDayStarted;
            dayLogic.OnDayReset   += HandleDayReset;

            // Estado inicial: si currentSecond > 0 asumimos que ya está corriendo
            dayRunning = (dayLogic.currentSecond > 0);
        }
        else
        {
            Debug.LogWarning("[ItemGeneric02Ticker] No se encontró DayLogic en escena. El generador permanecerá pausado.");
            dayRunning = false;
        }

        // Desfase inicial aleatorio para desincronizar copias
        temporizador = Random.Range(0f, this.intervalo);

        // AudioSource 2D interno
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D
        audioSource.volume = 1f;

        inicializado = true;

#if UNITY_EDITOR
        name = $"ItemGeneric02Ticker ({this.rangoDinero.x}-{this.rangoDinero.y} / {this.intervalo:0.##}s)";
#endif
    }

    private void OnDestroy()
    {
        if (dayLogic != null)
        {
            dayLogic.OnDayStarted -= HandleDayStarted;
            dayLogic.OnDayReset   -= HandleDayReset;
        }
    }

    private void HandleDayStarted()
    {
        dayRunning = true;
        // No reseteamos temporizador: reanuda donde quedó
    }

    private void HandleDayReset()
    {
        dayRunning = false;
        // Optional: si quieres que al reiniciar quede un nuevo desfase aleatorio:
        // temporizador = Random.Range(0f, intervalo);
    }

    private void Update()
    {
        if (!inicializado || stats == null)
            return;

        // 🔒 Pausado si el día no corre
        if (!dayRunning)
            return;

        temporizador += Time.deltaTime;
        if (temporizador >= intervalo)
        {
            temporizador -= intervalo;
            Activar();
        }
    }

    private void Activar()
    {
        int cantidad = CalcularCantidadConSuerte(rangoDinero.x, rangoDinero.y, stats.GetSuerteTotal());
        stats.dinero += cantidad;

        // Debug opcional:
        // Debug.Log($"[ItemGeneric02Ticker] +¥{cantidad} (suerte {stats.GetSuerteTotal()}) → total: ¥{stats.dinero}");

        // 🎵 Sonido por activación con pitch aleatorio
        if (sonido != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(pitchMin, pitchMax);
            audioSource.volume = Mathf.Max(0f, volumenMult);
            audioSource.PlayOneShot(sonido);
        }
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
            float w = Mathf.Pow(x, alpha);
            w = Mathf.Lerp(piso, 1f, w);
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