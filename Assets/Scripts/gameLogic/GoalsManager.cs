using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class GoalsManager : MonoBehaviour
{
    [Header("Referencias principales")]
    [SerializeField] private DayLogic dayLogic;
    [SerializeField] private GameStats gameStats;

    [Header("Textos dinámicos (UI)")]
    [SerializeField] private TextMeshProUGUI textoIngresos;
    [SerializeField] private TextMeshProUGUI textoImpuestos;
    [SerializeField] private TextMeshProUGUI textoBalance;

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

    private int ultimoDiaCalculado = -1;
    private float impuestoFinal;

    private void Start()
    {
        if (dayLogic == null)
            dayLogic = FindFirstObjectByType<DayLogic>();
        if (gameStats == null)
            gameStats = FindFirstObjectByType<GameStats>();

        Debug.Log("[GoalsManager] Iniciado (lee la suerte máxima directamente desde GameStats).");
    }

    private void Update()
    {
        if (gameStats == null || dayLogic == null) return;

        int diaActual = dayLogic.currentDay;
        float ingresos = gameStats.GetDineroTotal();

        if (diaActual != ultimoDiaCalculado)
        {
            ultimoDiaCalculado = diaActual;

            // === 1. Calcular exponente del día ===
            float exponente = exponenteInicial + incrementoExponentePorDia * (diaActual - 1);

            // === 2. Crecimiento exponencial ===
            float crecimiento = Mathf.Pow(impuestoBase, exponente) * multiplicadorCrecimiento;
            float impuestoBaseCrecido = impuestoBase + crecimiento;

            // === 3. Obtener suerte total y máxima directamente desde GameStats ===
            int suerteTotal = gameStats.GetSuerteTotal();
            float suerteMaximaGlobal = gameStats.GetSuerteMaximaTotal(); // 👈 Nueva función que debes tener en GameStats
            if (suerteMaximaGlobal <= 0f) suerteMaximaGlobal = 1f;

            float suerteNorm = Mathf.Clamp01(suerteTotal / suerteMaximaGlobal);
            float porcentajeSuerte = suerteNorm * 100f;

            // === 4. Calcular deuda base según suerte ===
            float deudaBase = deudaExtraMin + (deudaExtraMax - deudaExtraMin) * suerteNorm;
            deudaBase += Random.Range(-(deudaExtraMax - deudaExtraMin) * 0.15f, (deudaExtraMax - deudaExtraMin) * 0.15f);
            deudaBase = Mathf.Clamp(deudaBase, deudaExtraMin, deudaExtraMax);

            // === 5. Determinar signo de la deuda ===
            bool restaDeuda = false;
            string motivoSuerte = "";

            if (useGuaranteedLuckZones)
            {
                if (porcentajeSuerte >= suerteAltaGarantizada)
                {
                    restaDeuda = true;
                    motivoSuerte = "garantía de buena suerte";
                }
                else if (porcentajeSuerte <= suerteBajaGarantizada)
                {
                    restaDeuda = false;
                    motivoSuerte = "garantía de mala suerte";
                }
                else
                {
                    restaDeuda = Random.value < suerteNorm;
                    motivoSuerte = $"azar normal ({suerteNorm * 100f:F0}% chance de restar)";
                }
            }
            else
            {
                restaDeuda = Random.value < suerteNorm;
                motivoSuerte = $"azar normal ({suerteNorm * 100f:F0}% chance de restar)";
            }

            // === 6. Aplicar signo y calcular impuesto final ===
            float deudaFinal = restaDeuda ? -deudaBase : deudaBase;
            impuestoFinal = Mathf.Max(0f, impuestoBaseCrecido + deudaFinal);

            // === 7. Log detallado ===
            Debug.Log(
                $"[GoalsManager] Día {diaActual}\n" +
                $"---------------------------------\n" +
                $"• Exponente usado: {exponente:F3}\n" +
                $"• Crecimiento base: {crecimiento:F2}\n" +
                $"• Impuesto base crecido: {impuestoBaseCrecido:F2}\n" +
                $"• Suerte total: {suerteTotal} / {suerteMaximaGlobal} ({porcentajeSuerte:F1}%)\n" +
                $"• Deuda base generada: {deudaBase:F2}\n" +
                $"• Resultado: {(restaDeuda ? "RESTA" : "SUMA")} ({motivoSuerte})\n" +
                $"• Deuda final aplicada: {(restaDeuda ? "-" : "+")}{Mathf.Abs(deudaBase):F2}\n" +
                $"• Impuesto final calculado: {impuestoFinal:F2}\n"
            );
        }

        // === 8. Actualizar UI ===
        if (textoIngresos != null)
            textoIngresos.text = $"{ingresos:N0}";

        if (textoImpuestos != null)
            textoImpuestos.text = $"{impuestoFinal:N0}";

        if (textoBalance != null)
        {
            float balanceVisible = ingresos - impuestoFinal;
            textoBalance.text = $"{balanceVisible:N0}";
        }
    }
}