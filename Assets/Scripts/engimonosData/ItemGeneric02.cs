using UnityEngine;

//
// ItemGeneric02.cs
//
// - Usa tu base funcional con el Ticker inyectado en GameStats.
// - Cada copia del ítem añade su propio Ticker independiente (acumulable).
// - La cantidad generada está sesgada por la Suerte (0–100), con 100 => siempre máximo.
// - Para <100, mantiene margen de error (no “rota”).
//

/// <summary>
/// Item genérico #02:
/// - Al comprarse, agrega un generador pasivo de dinero al objeto con GameStats.
/// - Cada copia añade SU PROPIO generador con su propio temporizador (acumulable e independiente).
/// - El generador sigue funcionando aunque el objeto de tienda sea destruido.
/// </summary>
public class ItemGeneric02 : MonoBehaviour, IEngimonoApply
{
    [Header("Configuración del efecto")]
    [Tooltip("Tiempo entre cada entrega de dinero (en segundos).")]
    [SerializeField] private float intervalo = 5f;

    [Tooltip("Rango de dinero aleatorio que se entrega cada vez (incluye extremos).")]
    [SerializeField] private Vector2Int rangoDinero = new Vector2Int(2, 12);

    /// <summary>
    /// La tienda llamará a este método al completar la compra.
    /// Aquí inyectamos un ticker en el GameObject objetivo que tiene GameStats.
    /// </summary>
    public void AplicarEfecto(GameObject objetivo)
    {
        if (objetivo == null)
        {
            Debug.LogWarning("[ItemGeneric02] Objetivo nulo al aplicar efecto.");
            return;
        }

        GameStats stats = objetivo.GetComponent<GameStats>();
        if (stats == null)
        {
            Debug.LogWarning("[ItemGeneric02] No se encontró GameStats en el objetivo.");
            return;
        }

        // Agregamos un ticker NUEVO por cada copia del ítem (acumulable).
        ItemGeneric02Ticker ticker = objetivo.AddComponent<ItemGeneric02Ticker>();
        ticker.Init(stats, intervalo, rangoDinero);

        Debug.Log($"[ItemGeneric02] Generador creado: cada {intervalo:0.##}s dará ¥{rangoDinero.x}–¥{rangoDinero.y}, con suerte aplicada.");
    }
}

/// <summary>
/// Ticker independiente por copia del ítem.
/// Vive en el mismo GameObject que GameStats (inyectado en AplicarEfecto).
/// </summary>
public class ItemGeneric02Ticker : MonoBehaviour
{
    private GameStats stats;
    private float intervalo = 5f;
    private Vector2Int rangoDinero = new Vector2Int(2, 12);
    private float temporizador;

    private bool inicializado = false;

    /// <summary>
    /// Inicializa los parámetros del ticker. Debe llamarse inmediatamente tras AddComponent.
    /// </summary>
    public void Init(GameStats stats, float intervalo, Vector2Int rango)
    {
        this.stats = stats;
        this.intervalo = Mathf.Max(0.1f, intervalo); // seguridad mínima
        this.rangoDinero = new Vector2Int(
            Mathf.Min(rango.x, rango.y),
            Mathf.Max(rango.x, rango.y)
        );

        temporizador = 0f;
        inicializado = true;
        enabled = true;

#if UNITY_EDITOR
        name = $"ItemGeneric02Ticker ({this.rangoDinero.x}-{this.rangoDinero.y} / {this.intervalo:0.##}s)";
#endif
    }

    private void Update()
    {
        if (!inicializado || stats == null)
        {
            // Si algo falla (por ejemplo, GameStats fue destruido), nos deshabilitamos.
            enabled = false;
            return;
        }

        temporizador += Time.deltaTime;
        if (temporizador >= intervalo)
        {
            // Conserva el exceso de tiempo (mejora precisión en framerates variables)
            temporizador -= intervalo;

            int cantidad = CalcularCantidadConSuerte(rangoDinero.x, rangoDinero.y, stats.GetSuerteTotal());
            stats.dinero += cantidad;

            Debug.Log($"[ItemGeneric02Ticker] Suerte {stats.GetSuerteTotal()} → +¥{cantidad}. Dinero total: {stats.dinero}");
        }
    }

    /// <summary>
    /// Devuelve una cantidad entre [min, max] sesgada por la suerte:
    /// - Suerte=100 → siempre max.
    /// - Suerte baja → distribución casi uniforme.
    /// - Suerte alta → sesgo fuerte hacia números altos, pero con margen (no determinista salvo 100).
    /// </summary>
    private int CalcularCantidadConSuerte(int min, int max, int suerte)
    {
        suerte = Mathf.Clamp(suerte, 0, 100);
        if (suerte >= 100) return max;

        int count = max - min + 1;                   // cantidad de valores discretos
        float t = suerte / 100f;                     // 0..1

        // Exponente (alpha) crece suave con la suerte: 0 => uniforme, ~4.5 en 99 => sesgo fuerte arriba.
        float alpha = Mathf.Lerp(0f, 4.5f, Mathf.SmoothStep(0f, 1f, t));

        // Piso de probabilidad para que, incluso con suerte alta, valores bajos aún tengan chance.
        const float piso = 0.02f;

        // Construimos pesos crecientes (valores altos más pesados).
        float total = 0f;
        float[] pesos = new float[count];
        for (int i = 0; i < count; i++)
        {
            // i=0 -> min, i=count-1 -> max
            float x = (i + 1) / (float)count;           // 1/count .. 1
            float w = Mathf.Pow(x, alpha);              // curva de potencia
            w = Mathf.Lerp(piso, 1f, w);                // aplicamos piso
            pesos[i] = w;
            total += w;
        }

        // Ruleta ponderada
        float r = Random.value * total;
        float acum = 0f;
        for (int i = 0; i < count; i++)
        {
            acum += pesos[i];
            if (r <= acum)
                return min + i;
        }
        return max; // por seguridad numérica
    }
}