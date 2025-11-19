using UnityEngine;

[DisallowMultipleComponent]
public class GoalsManager : MonoBehaviour
{
    [Header("Referencias principales")]
    [SerializeField] private DayLogic dayLogic;
    [SerializeField] private GameStats gameStats;

    [Header("Pantalla de Game Over")]
    [SerializeField] private EndScreenBehaviour endScreenBehaviour;

    [Header("Configuracion de impuestos")]
    public float impuestoBase = 10245f;
    public float exponenteInicial = 0.8f;
    public float incrementoExponentePorDia = 0.15f;
    public float multiplicadorCrecimiento = 0.1f;

    [Header("Configuracion de deuda aleatoria")]
    public float deudaExtraMin = 350f;
    public float deudaExtraMax = 1000f;

    [Header("Sistema de suerte extrema")]
    public bool useGuaranteedLuckZones = true;
    [Range(0, 100)] public float suerteAltaGarantizada = 75f;
    [Range(0, 100)] public float suerteBajaGarantizada = 15f;

    private float impuestoFinal;
    private float ultimoBalance;
    private float ultimosIngresos;
    private bool jugadorPerdioHoy = false;
    private bool cierreAplicado = false;

    private void Start()
    {
        if (dayLogic == null)
            dayLogic = FindFirstObjectByType<DayLogic>();

        if (gameStats == null)
            gameStats = FindFirstObjectByType<GameStats>();

        if (endScreenBehaviour == null)
            endScreenBehaviour = FindFirstObjectByType<EndScreenBehaviour>();

        Debug.Log("[GoalsManager] Start completado.");
    }

    // Calculo principal del dia
    public void CalcularResultadosDia(int diaActual)
    {
        if (gameStats == null)
        {
            Debug.LogError("[GoalsManager] GameStats es null en CalcularResultadosDia.");
            return;
        }

        jugadorPerdioHoy = false;

        float ingresos = gameStats.GetDineroTotal();
        ultimosIngresos = ingresos;

        float exponente = exponenteInicial + incrementoExponentePorDia * (diaActual - 1);
        float crecimiento = Mathf.Pow(impuestoBase, exponente) * multiplicadorCrecimiento;
        float impuestoBaseCrecido = impuestoBase + crecimiento;

        int suerteTotal = gameStats.GetSuerteTotal();
        float suerteMaximaGlobal = gameStats.GetSuerteMaximaTotal();
        if (suerteMaximaGlobal <= 0f) suerteMaximaGlobal = 1f;

        float suerteNorm = Mathf.Clamp01(suerteTotal / suerteMaximaGlobal);
        float porcentajeSuerte = suerteNorm * 100f;

        float deudaBase = deudaExtraMin + (deudaExtraMax - deudaExtraMin) * suerteNorm;
        deudaBase += Random.Range(-(deudaExtraMax - deudaExtraMin) * 0.15f,
                                  (deudaExtraMax - deudaExtraMin) * 0.15f);
        deudaBase = Mathf.Clamp(deudaBase, deudaExtraMin, deudaExtraMax);

        bool restaDeuda;

        if (useGuaranteedLuckZones)
        {
            if (porcentajeSuerte >= suerteAltaGarantizada)
                restaDeuda = true;
            else if (porcentajeSuerte <= suerteBajaGarantizada)
                restaDeuda = false;
            else
                restaDeuda = Random.value < suerteNorm;
        }
        else
        {
            restaDeuda = Random.value < suerteNorm;
        }

        float deudaFinal = restaDeuda ? -deudaBase : deudaBase;
        impuestoFinal = Mathf.Max(0f, impuestoBaseCrecido + deudaFinal);

        ultimoBalance = ingresos - impuestoFinal;
        jugadorPerdioHoy = (ultimoBalance < 0f);

        Debug.Log($"[GoalsManager] Dia {diaActual} calculado. Ingresos={ingresos}, Impuestos={impuestoFinal}, Balance={ultimoBalance}, Perdio={jugadorPerdioHoy}");

        if (endScreenBehaviour != null)
        {
            Debug.Log("[GoalsManager] Enviando resultado del dia a EndScreenBehaviour.");
            endScreenBehaviour.SetResultadoDelDia(jugadorPerdioHoy);
        }
        else
        {
            Debug.LogWarning("[GoalsManager] endScreenBehaviour es null, no puedo enviar resultado.");
        }
    }

    // Cierre final del dia
    public void IniciarCierreFinal()
    {
        if (cierreAplicado)
        {
            Debug.Log("[GoalsManager] IniciarCierreFinal llamado pero ya estaba aplicado.");
            return;
        }

        cierreAplicado = true;

        int nuevoDinero = Mathf.Max(0, Mathf.RoundToInt(ultimoBalance));
        gameStats.dinero = nuevoDinero;

        Debug.Log("[GoalsManager] Cierre final aplicado. Dinero final: " + nuevoDinero + ". Perdio=" + jugadorPerdioHoy);

        if (jugadorPerdioHoy && endScreenBehaviour != null)
        {
            Debug.Log("[GoalsManager] Jugador perdio, activando EndScreen (diario).");
            endScreenBehaviour.ActivarEndScreen();
        }
    }

    // Getters
    public float GetIngresosFinales() => ultimosIngresos;
    public float GetImpuestosFinales() => impuestoFinal;
    public float GetBalanceFinal() => ultimoBalance;
    public bool JugadorPerdio() => jugadorPerdioHoy;
}