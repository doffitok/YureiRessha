using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class RatingManager : MonoBehaviour
{
    [Header("Referencias principales")]
    [SerializeField] private DayLogic dayLogic;
    [SerializeField] private GameStats gameStats;
    [SerializeField] private EndScreenBehaviour endScreenBehaviour;

    [Header("Configuración de Rating")]
    public float ratingBase = 3.0f;
    public float ratingMaximo = 5.0f;
    public float ratingMinimoAprobacion = 2.5f;
    public float incrementoDificultadPorDia = 0.1f;

    [Header("Feedback de Pasajeros")]
    public List<PassengerFeedbackSO> feedbackTemplates;
    
    // Variables temporales para pruebas - luego las mueves a GameStats
    private int estacionesCompletadasTemp = 0;
    private int pasajerosAtendidosTemp = 0;
    private int erroresTemp = 0;

    private float ratingFinal;
    private float ratingActualCalculado;
    private List<PassengerFeedback> feedbacksDelDia = new List<PassengerFeedback>();
    private bool jugadorPerdioHoy = false;
    private bool cierreAplicado = false;

    private void Start()
    {
        // Buscar referencias si no están asignadas
        if (dayLogic == null)
            dayLogic = FindFirstObjectByType<DayLogic>();

        if (gameStats == null)
            gameStats = FindFirstObjectByType<GameStats>();

        if (endScreenBehaviour == null)
            endScreenBehaviour = FindFirstObjectByType<EndScreenBehaviour>();

        Debug.Log("[RatingManager] Inicializado correctamente");
    }

    // Método para que otros scripts registren acciones
    public void RegistrarAccion(string tipoAccion, bool positiva = true, float impacto = 0.2f)
    {
        switch (tipoAccion.ToLower())
        {
            case "estacion_completada":
                estacionesCompletadasTemp++;
                AddFeedback("Sistema", $"Estación completada correctamente", impacto, true);
                break;
                
            case "pasajero_atendido":
                pasajerosAtendidosTemp++;
                AddFeedback("Sistema", $"Pasajero atendido", 0.1f, true);
                break;
                
            case "error":
                erroresTemp++;
                AddFeedback("Sistema", $"Error cometido", -impacto, false);
                break;
                
            default:
                AddFeedback("Sistema", $"{tipoAccion}", positiva ? impacto : -impacto, positiva);
                break;
        }
    }

    // Método principal - llamar al final del día
    public void CalcularRatingDelDia(int diaActual)
    {
        jugadorPerdioHoy = false;
        feedbacksDelDia.Clear();

        // 1. CALCULAR RATING ACTUAL (usando variables temporales)
        ratingActualCalculado = CalcularRatingDesdeAcciones();

        // 2. GENERAR FEEDBACK DE PASAJEROS (opcional)
        if (feedbackTemplates != null && feedbackTemplates.Count > 0)
        {
            GenerarFeedbacksPasajeros(diaActual);
        }

        // 3. CALCULAR RATING FINAL
        float ratingBaseAjustado = ratingBase - (incrementoDificultadPorDia * (diaActual - 1));
        float sumaFeedback = CalcularSumaFeedbacks();
        
        ratingFinal = Mathf.Clamp(
            ratingBaseAjustado + sumaFeedback, 
            0f, 
            ratingMaximo
        );

        // Verificar si perdió
        jugadorPerdioHoy = (ratingFinal < ratingMinimoAprobacion);

        Debug.Log($"[RatingManager] Día {diaActual} - " +
                  $"Estaciones: {estacionesCompletadasTemp}, " +
                  $"Pasajeros: {pasajerosAtendidosTemp}, " +
                  $"Errores: {erroresTemp}, " +
                  $"Rating Final: {ratingFinal:F1}/5.0");

        // Mostrar resultados en consola
        MostrarResumenConsola();

        if (endScreenBehaviour != null)
        {
            endScreenBehaviour.SetResultadoDelDia(jugadorPerdioHoy);
        }
    }

    private float CalcularRatingDesdeAcciones()
    {
        float rating = ratingBase;
        
        // Bonus por estaciones completadas
        rating += estacionesCompletadasTemp * 0.3f;
        
        // Bonus por pasajeros atendidos
        rating += pasajerosAtendidosTemp * 0.1f;
        
        // Penalización por errores
        rating -= erroresTemp * 0.5f;
        
        return Mathf.Clamp(rating, 0f, ratingMaximo);
    }

    private void GenerarFeedbacksPasajeros(int diaActual)
    {
        int numFeedbacks = Random.Range(2, 4);

        for (int i = 0; i < numFeedbacks; i++)
        {
            PassengerFeedbackSO template = feedbackTemplates[Random.Range(0, feedbackTemplates.Count)];
            
            // Decidir si el feedback es positivo basado en el desempeño
            bool esPositivo = (ratingActualCalculado > 3.0f) || Random.value > 0.5f;
            
            string nombre = template != null ? template.passengerName : $"Pasajero {i+1}";
            string texto = esPositivo ? 
                "¡Buen viaje! El servicio fue aceptable." : 
                "El viaje podría mejorar...";
            
            float contribucion = esPositivo ? 
                Random.Range(0.1f, 0.25f) : 
                Random.Range(-0.25f, -0.1f);
            
            AddFeedback(nombre, texto, contribucion, esPositivo);
        }
    }

    private void AddFeedback(string nombre, string texto, float contribucion, bool positivo)
    {
        PassengerFeedback feedback = new PassengerFeedback
        {
            passengerName = nombre,
            feedbackText = texto,
            ratingContribution = contribucion,
            isPositive = positivo
        };
        
        feedbacksDelDia.Add(feedback);
        Debug.Log($"[Feedback] {nombre}: {texto} ({contribucion:F2}★)");
    }

    private float CalcularSumaFeedbacks()
    {
        float total = 0f;
        foreach (var feedback in feedbacksDelDia)
        {
            total += feedback.ratingContribution;
        }
        return total;
    }

    private void MostrarResumenConsola()
    {
        Debug.Log("=== RESUMEN DEL DÍA ===");
        Debug.Log($"Rating Calculado: {ratingActualCalculado:F1}/5.0");
        Debug.Log($"Rating Final: {ratingFinal:F1}/5.0");
        Debug.Log($"Mínimo Requerido: {ratingMinimoAprobacion:F1}/5.0");
        Debug.Log($"Resultado: {(jugadorPerdioHoy ? "REPROBADO" : "APROBADO")}");
        Debug.Log("======================");
    }

    public void IniciarCierreFinal()
    {
        if (cierreAplicado) return;
        cierreAplicado = true;

        // Aquí puedes guardar el rating en GameStats si añades la variable
        // Por ahora solo mostramos en consola
        
        Debug.Log($"[Cierre Final] Rating del día: {ratingFinal:F1}/5.0");

        if (jugadorPerdioHoy && endScreenBehaviour != null)
        {
            endScreenBehaviour.ActivarEndScreen();
        }

        // Resetear variables para el próximo día
        ResetearContadoresDiarios();
    }

    private void ResetearContadoresDiarios()
    {
        estacionesCompletadasTemp = 0;
        pasajerosAtendidosTemp = 0;
        erroresTemp = 0;
        feedbacksDelDia.Clear();
    }

    // Getters para UI
    public float GetRatingActual() => ratingActualCalculado;
    public float GetRatingFinal() => ratingFinal;
    public List<PassengerFeedback> GetFeedbacksDelDia() => new List<PassengerFeedback>(feedbacksDelDia);
    public bool JugadorPerdio() => jugadorPerdioHoy;

    // Métodos para obtener estadísticas (para UI)
    public int GetEstacionesCompletadas() => estacionesCompletadasTemp;
    public int GetPasajerosAtendidos() => pasajerosAtendidosTemp;
    public int GetErroresCometidos() => erroresTemp;

    [System.Serializable]
    public class PassengerFeedback
    {
        public string passengerName;
        public string feedbackText;
        public float ratingContribution;
        public bool isPositive;
    }
}