using UnityEngine;

[DisallowMultipleComponent]
public class GoalsManager : MonoBehaviour
{
    [Header("Referencias principales")]
    [SerializeField] private DayLogic dayLogic;
    [SerializeField] private GameStats gameStats;

    [Header("Configuración de impuestos")]
    public float impuestoBase = 10245f;
    public float exponenteInicial = 0.8f;
    public float incrementoExponentePorDia = 0.15f;
    public float multiplicadorCrecimiento = 0.1f;

    [Header("Configuración de deuda aleatoria")]
    public float deudaExtraMin = 350f;
    public float deudaExtraMax = 1000f;

    [Header("Sistema de suerte extrema")]
    public bool useGuaranteedLuckZones = true;
    [Range(0, 100)] public float suerteAltaGarantizada = 75f;
    [Range(0, 100)] public float suerteBajaGarantizada = 15f;

    //───────────────────────────────────────────────────────────────
    // Variables internas
    //───────────────────────────────────────────────────────────────
    private int ultimoDiaCalculado = -1;
    private float impuestoFinal;
    private float ultimoBalance;
    private float ultimosIngresos;

    // Flag de control para evitar múltiples cierres
    private bool cierreAplicado = false;

    private void Start()
    {
        if (dayLogic == null)
            dayLogic = FindFirstObjectByType<DayLogic>();
        if (gameStats == null)
            gameStats = FindFirstObjectByType<GameStats>();

        Debug.Log("[GoalsManager] Iniciado (modo cálculo puro).");
    }

    //───────────────────────────────────────────────────────────────
    // Cálculo principal del día
    //───────────────────────────────────────────────────────────────
    public void CalcularResultadosDia(int diaActual)
    {
        if (gameStats == null) return;

        cierreAplicado = false; // reset del cierre final

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
        deudaBase += Random.Range(-(deudaExtraMax - deudaExtraMin) * 0.15f, (deudaExtraMax - deudaExtraMin) * 0.15f);
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
            restaDeuda = Random.value < suerteNorm;

        float deudaFinal = restaDeuda ? -deudaBase : deudaBase;
        impuestoFinal = Mathf.Max(0f, impuestoBaseCrecido + deudaFinal);

        ultimoBalance = ingresos - impuestoFinal;
        ultimoDiaCalculado = diaActual;

        Debug.Log($"[GoalsManager] Día {diaActual} terminado → Ingresos: {ingresos:F0}, Impuestos: {impuestoFinal:F0}, Balance: {ultimoBalance:F0}");
    }

    //───────────────────────────────────────────────────────────────
    // Cierre final: aplica resultados, guarda, desbloquea continuar
    //───────────────────────────────────────────────────────────────
    public void IniciarCierreFinal()
    {
        if (gameStats == null || cierreAplicado)
            return;

        cierreAplicado = true;

        Debug.Log("[GoalsManager] 🧾 Iniciando cierre final del día...");

        float balance = GetBalanceFinal();

        // Aplicar el balance al dinero total del jugador
        int nuevoDinero = Mathf.Max(0, Mathf.RoundToInt(balance));
        gameStats.dinero = nuevoDinero;

        // Aquí podrías añadir guardado, estadísticas globales, etc.
        // Ejemplo:
        // SaveSystem.GuardarProgreso(diaActual, nuevoDinero, otrosDatos);

        Debug.Log($"[GoalsManager] ✅ Cierre aplicado → Dinero final del jugador: {nuevoDinero}");

        // 🔹 Lógica para notificar que ya puede aparecer el botón de "continuar"
        // Esto podría ser un evento, por ahora lo dejamos preparado:
        OnCierreFinalizado?.Invoke();
    }

    // Evento opcional para cuando se complete el cierre
    public event System.Action OnCierreFinalizado;

    //───────────────────────────────────────────────────────────────
    // Getters públicos
    //───────────────────────────────────────────────────────────────
    public float GetIngresosFinales() => ultimosIngresos;
    public float GetImpuestosFinales() => impuestoFinal;
    public float GetBalanceFinal() => ultimoBalance;
}