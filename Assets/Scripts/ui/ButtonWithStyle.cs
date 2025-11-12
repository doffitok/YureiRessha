using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonWithStyle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Referencias")]
    public Image highlightFrame;
    public TextMeshProUGUI nameText;
    
    [Header("Configuración Animación")]
    public float animationSpeed = 8f;
    public float scaleMultiplier = 1.08f;
    public float frameFadeSpeed = 10f;
    
    [Header("Efecto Pulse")]
    public bool enablePulseEffect = true;
    public float pulseSpeed = 2f;
    public float pulseIntensity = 0.1f;
    
    [Header("Selection Settings")]
    public Color selectedTextColor = Color.yellow;
    public Color normalTextColor = Color.white;
    public float selectedAlpha = 0.6f;
    public float hoverAlpha = 0.4f;
    
    private Vector3 originalScale;
    private Vector3 originalFrameScale;
    private Vector3 targetScale;
    private bool isHovering;
    private bool isForcedHighlight;
    private Color frameOriginalColor;
    private Color frameTargetColor;

    void Start()
    {
        // Guardar escala original
        originalScale = transform.localScale;
        targetScale = originalScale;
        
        // Configurar frame
        if (highlightFrame != null)
        {
            frameOriginalColor = highlightFrame.color;
            frameTargetColor = frameOriginalColor;
            frameTargetColor.a = 0f; // Inicialmente transparente
            highlightFrame.color = frameTargetColor;
            
            // Guardar escala original del frame
            originalFrameScale = highlightFrame.transform.localScale;
        }

        // Configurar color inicial del texto
        if (nameText != null)
        {
            nameText.color = normalTextColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isForcedHighlight) return; // No hacer nada si está forzado
        
        isHovering = true;
        targetScale = originalScale * scaleMultiplier;
        
        if (highlightFrame != null)
        {
            frameTargetColor.a = hoverAlpha;
        }
        
        // Efecto en el texto (opcional)
        if (nameText != null)
        {
            nameText.color = selectedTextColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isForcedHighlight) return; // No hacer nada si está forzado
        
        isHovering = false;
        targetScale = originalScale;
        
        if (highlightFrame != null)
        {
            frameTargetColor.a = 0f;
        }
        
        // Restaurar texto solo si no está seleccionado
        if (nameText != null && !isForcedHighlight)
        {
            nameText.color = normalTextColor;
        }
    }

    void Update()
    {
        // Animación suave de escala del botón
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
        
        // Animación suave del frame
        if (highlightFrame != null)
        {
            highlightFrame.color = Color.Lerp(highlightFrame.color, frameTargetColor, Time.deltaTime * frameFadeSpeed);
            
            // Efecto de "pulso" sutil cuando está en hover o seleccionado
            if ((isHovering || isForcedHighlight) && enablePulseEffect)
            {
                float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
                float scaleModifier = 1f + (pulse * pulseIntensity);
                highlightFrame.transform.localScale = originalFrameScale * scaleModifier;
            }
            else
            {
                // Volver suavemente a la escala original
                highlightFrame.transform.localScale = Vector3.Lerp(
                    highlightFrame.transform.localScale, 
                    originalFrameScale, 
                    Time.deltaTime * animationSpeed
                );
            }
        }
    }

    // 🔥 NUEVO MÉTODO: Para resaltado forzado (selección)
    public void ForceHighlight(bool highlight)
    {
        isForcedHighlight = highlight;
        
        if (highlight)
        {
            // Simular hover pero más intenso
            targetScale = originalScale * (scaleMultiplier * 1.1f);
            
            if (highlightFrame != null)
            {
                frameTargetColor.a = selectedAlpha; // Más opaco para selección
            }
            
            if (nameText != null)
            {
                nameText.color = selectedTextColor;
            }
        }
        else
        {
            // Resetear a normal
            isHovering = false;
            targetScale = originalScale;
            
            if (highlightFrame != null)
            {
                frameTargetColor.a = 0f;
            }
            
            if (nameText != null)
            {
                nameText.color = normalTextColor;
            }
        }
    }

    // 🔥 NUEVO MÉTODO: Para resetear completamente
    public void ResetVisuals()
    {
        isHovering = false;
        isForcedHighlight = false;
        transform.localScale = originalScale;
        targetScale = originalScale;
        
        if (highlightFrame != null)
        {
            frameTargetColor.a = 0f;
            highlightFrame.color = frameTargetColor;
            highlightFrame.transform.localScale = originalFrameScale;
        }
        
        if (nameText != null)
        {
            nameText.color = normalTextColor;
        }
    }

    // 🔥 NUEVO: Método para cambiar colores dinámicamente
    public void SetColors(Color newSelectedColor, Color newNormalColor)
    {
        selectedTextColor = newSelectedColor;
        normalTextColor = newNormalColor;

        // Aplicar cambios inmediatamente
        if (nameText != null)
        {
            if (isHovering || isForcedHighlight)
                nameText.color = selectedTextColor;
            else
                nameText.color = normalTextColor;
        }
    }

    // 🔥 NUEVO: Método para actualizar referencias si se cambian en tiempo de ejecución
    public void UpdateReferences(Image newHighlightFrame = null, TextMeshProUGUI newNameText = null)
    {
        if (newHighlightFrame != null)
        {
            highlightFrame = newHighlightFrame;
            frameOriginalColor = highlightFrame.color;
            originalFrameScale = highlightFrame.transform.localScale;
        }
        
        if (newNameText != null)
        {
            nameText = newNameText;
        }
        
        ResetVisuals();
    }
}