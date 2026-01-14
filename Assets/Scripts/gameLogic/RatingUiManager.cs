using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RatingUIManager_TMP : MonoBehaviour
{
    [Header("Referencias UI - TMP")]
    public GameObject ratingPanel;
    public TextMeshProUGUI ratingActualText;
    public TextMeshProUGUI ratingFinalText;
    public Image ratingFillImage;
    public Button continueButton;
    
    [Header("Sección de Opiniones - TMP")]
    public GameObject feedbackSection;
    public TextMeshProUGUI feedbackTextTemplate; // Prefab TMP
    public Transform feedbackContentParent;
    public ScrollRect feedbackScrollRect;
    
    [Header("Estadísticas - TMP")]
    public TextMeshProUGUI statsEstacionesText;
    public TextMeshProUGUI statsPasajerosText;
    public TextMeshProUGUI statsErroresText;
    
    [Header("Colores")]
    public Color colorExcelente = Color.green;
    public Color colorBueno = new Color(0.2f, 0.8f, 0.2f);
    public Color colorRegular = Color.yellow;
    public Color colorMalo = Color.red;
    
    private RatingManager ratingManager;
    
    void Start()
    {
        ratingManager = FindObjectOfType<RatingManager>();
        
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }
        
        // Ocultar panel al inicio
        if (ratingPanel != null)
            ratingPanel.SetActive(false);
            
        if (feedbackSection != null)
            feedbackSection.SetActive(false);
    }
    
    // Método público para mostrar cuando termine el día
    public void MostrarResumenDelDia()
    {
        if (ratingManager == null) 
        {
            Debug.LogError("RatingManager no encontrado!");
            return;
        }
        
        if (ratingPanel != null)
        {
            ratingPanel.SetActive(true);
            Debug.Log("Panel de rating TMP ACTIVADO");
        }
        
        // Obtener datos
        float ratingActual = ratingManager.GetRatingActual();
        float ratingFinal = ratingManager.GetRatingFinal();
        var feedbacks = ratingManager.GetFeedbacksDelDia();
        
        // 1. SECCIÓN: RATING ACTUAL
        if (ratingActualText != null)
        {
            ratingActualText.text = $"<b>RATING ACTUAL</b>\n{ratingActual:F1}/5.0";
            ratingActualText.color = GetColorForRating(ratingActual);
        }
        
        // 2. SECCIÓN: ESTADÍSTICAS
        MostrarEstadisticas();
        
        // 3. SECCIÓN: OPINIONES DE PASAJEROS
        MostrarOpinionesPasajeros(feedbacks);
        
        // 4. SECCIÓN: RATING FINAL
        if (ratingFinalText != null)
        {
            string resultado = ratingFinal >= 2.5f ? 
                "<color=green>✓ APROBADO</color>" : 
                "<color=red>✗ REPROBADO</color>";
            
            ratingFinalText.text = $"<b>RATING FINAL</b>\n" +
                                  $"<size=36>{ratingFinal:F1}/5.0</size>\n" +
                                  $"{resultado}";
            ratingFinalText.color = GetColorForRating(ratingFinal);
        }
        
        // 5. BARRA DE PROGRESO
        if (ratingFillImage != null)
        {
            ratingFillImage.fillAmount = ratingFinal / 5.0f;
            ratingFillImage.color = GetColorForRating(ratingFinal);
        }
    }
    
    private void MostrarEstadisticas()
    {
        if (ratingManager == null) return;
        
        if (statsEstacionesText != null)
            statsEstacionesText.text = $"Estaciones: {ratingManager.GetEstacionesCompletadas()}";
            
        if (statsPasajerosText != null)
            statsPasajerosText.text = $"Pasajeros: {ratingManager.GetPasajerosAtendidos()}";
            
        if (statsErroresText != null)
            statsErroresText.text = $"Errores: {ratingManager.GetErroresCometidos()}";
    }
    
    private void MostrarOpinionesPasajeros(System.Collections.Generic.List<RatingManager.PassengerFeedback> feedbacks)
    {
        if (feedbackSection == null || feedbackTextTemplate == null || feedbackContentParent == null)
        {
            Debug.LogWarning("Faltan referencias TMP para opiniones");
            return;
        }
        
        feedbackSection.SetActive(true);
        
        // Limpiar opiniones anteriores
        foreach (Transform child in feedbackContentParent)
        {
            Destroy(child.gameObject);
        }
        
        // Mostrar cada feedback con TMP
        foreach (var feedback in feedbacks)
        {
            TextMeshProUGUI feedbackText = Instantiate(feedbackTextTemplate, feedbackContentParent);
            
            string icono = feedback.isPositive ? "✓" : "✗";
            Color color = feedback.isPositive ? Color.green : Color.red;
            string signo = feedback.ratingContribution >= 0 ? "+" : "";
            
            feedbackText.text = $"{icono} <b>{feedback.passengerName}:</b>\n" +
                               $"<size=12>{feedback.feedbackText}</size>\n" +
                               $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>" +
                               $"{signo}{feedback.ratingContribution:F1}★</color>";
            
            feedbackText.color = color;
            feedbackText.gameObject.SetActive(true);
        }
        
        // Scroll al inicio
        if (feedbackScrollRect != null)
            feedbackScrollRect.verticalNormalizedPosition = 1f;
    }
    
    private Color GetColorForRating(float rating)
    {
        if (rating >= 4.5f) return colorExcelente;
        if (rating >= 3.5f) return colorBueno;
        if (rating >= 2.5f) return colorRegular;
        return colorMalo;
    }
    
    private void OnContinueClicked()
    {
        Debug.Log("Continuar presionado - Cerrando panel TMP");
        
        if (ratingManager != null)
        {
            ratingManager.IniciarCierreFinal();
        }
        
        OcultarPanel();
    }
    
    public void OcultarPanel()
    {
        if (ratingPanel != null)
            ratingPanel.SetActive(false);
    }
}