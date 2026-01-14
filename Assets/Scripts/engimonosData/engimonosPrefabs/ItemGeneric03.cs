using UnityEngine;

////////////////////////////////////////////////////////////////////////////////////////////
// este item convierte automaticamente una pequena fraccion del dinero total del jugador
// en suerte pasiva adicional
// cada copia del item agrega su propio conversor independiente
// el efecto se aplica de forma pasiva sin alterar los valores base del jugador
////////////////////////////////////////////////////////////////////////////////////////////

public class ItemGeneric03 : MonoBehaviour, IEngimonoApply
{
    ////////////////////////////////////////////////////////////////////////////////////////////
    // configuracion del efecto
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Configuracion del efecto")]
    [Tooltip("Porcentaje de dinero convertido a suerte por ejemplo 0.005 para 0.005%")]
    [SerializeField] private float porcentajeConversion = 0.00005f;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // aplica el efecto al objetivo asignado
    ////////////////////////////////////////////////////////////////////////////////////////////
    public void AplicarEfecto(GameObject objetivo)
    {
        if (objetivo == null)
        {
            Debug.LogWarning("[ItemGeneric03] objetivo nulo al aplicar efecto");
            return;
        }

        GameStats stats = objetivo.GetComponent<GameStats>();
        if (stats == null)
        {
            Debug.LogWarning("[ItemGeneric03] no se encontro GameStats en el objetivo");
            return;
        }

        ItemGeneric03Ticker ticker = objetivo.AddComponent<ItemGeneric03Ticker>();
        ticker.Init(stats, porcentajeConversion);

        Debug.Log($"[ItemGeneric03] conversor creado convierte el {porcentajeConversion * 100f}% del dinero en suerte pasiva");
    }
}

////////////////////////////////////////////////////////////////////////////////////////////
// componente pasivo que actualiza la suerte en funcion del dinero actual
////////////////////////////////////////////////////////////////////////////////////////////
public class ItemGeneric03Ticker : MonoBehaviour
{
    private GameStats stats;
    private float porcentaje;
    private bool inicializado = false;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // inicializa el componente con sus datos base
    ////////////////////////////////////////////////////////////////////////////////////////////
    public void Init(GameStats stats, float porcentajeConversion)
    {
        this.stats = stats;
        this.porcentaje = porcentajeConversion;
        inicializado = true;
        enabled = true;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // actualiza la suerte adicional segun el dinero del jugador
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void Update()
    {
        if (!inicializado || stats == null)
        {
            enabled = false;
            return;
        }

        float suerteCalculada = stats.GetDineroTotal() * porcentaje;
        int nuevaSuerteExtra = Mathf.FloorToInt(suerteCalculada);
        stats.suerteExtra = nuevaSuerteExtra;
    }
}