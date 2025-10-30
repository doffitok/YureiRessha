using UnityEngine;

//
// ItemGeneric03.cs
//
// - Convierte automáticamente el 0.005% del dinero total del jugador en suerte.
// - Este bono se actualiza cada frame (o cada X segundos, si quisieras optimizarlo).
// - Es acumulable: cada copia del ítem agrega su propio conversor.
// - La suerte extra se añade de forma pasiva (no modifica el valor base).
//

/// <summary>
/// Item genérico #03:
/// - Convierte el 0.005% del dinero total en suerte adicional.
/// - Cada copia añade un conversor independiente.
/// </summary>
public class ItemGeneric03 : MonoBehaviour, IEngimonoApply
{
    [Header("Configuración del efecto")]
    [Tooltip("Porcentaje de dinero convertido a suerte (por ejemplo, 0.005 para 0.005%).")]
    [SerializeField] private float porcentajeConversion = 0.00005f; // 0.005%

    public void AplicarEfecto(GameObject objetivo)
    {
        if (objetivo == null)
        {
            Debug.LogWarning("[ItemGeneric03] Objetivo nulo al aplicar efecto.");
            return;
        }

        GameStats stats = objetivo.GetComponent<GameStats>();
        if (stats == null)
        {
            Debug.LogWarning("[ItemGeneric03] No se encontró GameStats en el objetivo.");
            return;
        }

        // Añadimos un conversor pasivo al GameStats
        ItemGeneric03Ticker ticker = objetivo.AddComponent<ItemGeneric03Ticker>();
        ticker.Init(stats, porcentajeConversion);

        Debug.Log($"[ItemGeneric03] Conversor creado: convierte el {porcentajeConversion * 100f}% del dinero en suerte pasiva.");
    }
}

/// <summary>
/// Componente pasivo que actualiza la suerte en función del dinero actual.
/// </summary>
public class ItemGeneric03Ticker : MonoBehaviour
{
    private GameStats stats;
    private float porcentaje;
    private bool inicializado = false;

    private float suerteExtraActual = 0f;

    public void Init(GameStats stats, float porcentajeConversion)
    {
        this.stats = stats;
        this.porcentaje = porcentajeConversion;
        inicializado = true;
        enabled = true;
    }

    private void Update()
    {
        if (!inicializado || stats == null)
        {
            enabled = false;
            return;
        }

        // Calcula la suerte pasiva basada en el dinero actual
        float suerteCalculada = stats.GetDineroTotal() * porcentaje;

        // Diferencia respecto a la suerte anterior
        int nuevaSuerteExtra = Mathf.FloorToInt(suerteCalculada);

        // Aplicar el valor (sin acumulación infinita)
        stats.suerteExtra = nuevaSuerteExtra;

        // Debug opcional
        // Debug.Log($"[ItemGeneric03Ticker] Dinero: ¥{stats.GetDineroTotal()} → Suerte extra: +{nuevaSuerteExtra}");
    }
}