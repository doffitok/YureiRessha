using UnityEngine;

////////////////////////////////////////////////////////////////////////////////////////////
// GoalsManager
//
// Calcula los impuestos diarios en base al día actual del juego.
// - Usa un valor base (impuesto inicial del día 1)
// - Aplica un crecimiento exponencial que aumenta con cada día
// - Añade una deuda aleatoria según un rango mínimo y máximo multiplicado por el número de días
////////////////////////////////////////////////////////////////////////////////////////////

[DisallowMultipleComponent]
public class GoalsManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private DayLogic dayLogic;

    [Header("Configuración de impuestos")]
    [Tooltip("Impuesto base del primer día")]
    [Min(0f)] public float impuestoBase = 100f;

    [Tooltip("Exponente de incremento (crece exponencialmente cada día)")]
    public float exponenteBase = 1.05f;

    [Tooltip("Deuda extra mínima aleatoria por día")]
    public float deudaExtraMin = 5f;

    [Tooltip("Deuda extra máxima aleatoria por día")]
    public float deudaExtraMax = 15f;

    [Header("Resultado (solo lectura)")]
    [SerializeField] private int diaActual;
    [SerializeField] private float impuestoDelDia;
    [SerializeField] private float deudaFinal;

    private void Start()
    {
        if (dayLogic == null)
            dayLogic = FindFirstObjectByType<DayLogic>();

        if (dayLogic != null)
            dayLogic.OnDayEnded += CalcularImpuestoDia;
    }

    private void OnDestroy()
    {
        if (dayLogic != null)
            dayLogic.OnDayEnded -= CalcularImpuestoDia;
    }

    private void CalcularImpuestoDia()
    {
        diaActual = dayLogic.currentDay;

        // Impuesto base con crecimiento exponencial
        impuestoDelDia = Mathf.Ceil(impuestoBase * Mathf.Pow(exponenteBase, diaActual - 1));

        // Deuda aleatoria proporcional al número de días
        float deudaExtra = Random.Range(deudaExtraMin, deudaExtraMax) * diaActual;

        // Resultado total (redondeado hacia arriba)
        deudaFinal = Mathf.Ceil(impuestoDelDia + deudaExtra);

        Debug.Log($"[GoalsManager] Día {diaActual} | Impuesto base: {impuestoDelDia} | Extra: {deudaExtra} | Total: {deudaFinal}");
    }

    // Getters públicos si otro script necesita acceder
    public int DiaActual => diaActual;
    public float ImpuestoDelDia => impuestoDelDia;
    public float DeudaFinal => deudaFinal;
}