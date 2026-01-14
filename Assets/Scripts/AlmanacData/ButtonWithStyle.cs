using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class ButtonWithStyle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Persona 5 Style Effects")]
    public Image highlightFrame;
    public Image flashEffect;
    public TextMeshProUGUI nameText;
    
    [Header("Animation Settings")]
    public float fadeInDuration = 0.15f;
    public float fadeOutDuration = 0.25f;
    public float flashIntensity = 1.2f;
    public float flashSpeed = 3f;
    
    [Header("Colors")]
    public Color frameNormalColor = new Color(1, 1, 1, 0);
    public Color frameHighlightColor = new Color(1, 1, 1, 0.9f);
    public Color flashNormalColor = new Color(1, 1, 1, 0);
    public Color flashHighlightColor = new Color(1, 1, 1, 0.4f);
    public Color textNormalColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    public Color textHighlightColor = Color.white;
    
    private Coroutine currentAnimation;
    private bool isHighlighted = false;
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        InitializeElements();
        ForceZOrder();
    }

    // 🔥 NUEVO: Forzar el orden usando Canvas components
    private void ForceZOrder()
    {
        if (flashEffect != null)
        {
            // Método 1: Canvas con sorting order negativo
            Canvas flashCanvas = flashEffect.gameObject.GetComponent<Canvas>();
            if (flashCanvas == null) 
                flashCanvas = flashEffect.gameObject.AddComponent<Canvas>();
            
            flashCanvas.overrideSorting = true;
            flashCanvas.sortingOrder = -10; // Muy atrás
            
            // También añadir GraphicRaycaster si es necesario para interacción
            GraphicRaycaster flashRaycaster = flashEffect.gameObject.GetComponent<GraphicRaycaster>();
            if (flashRaycaster == null)
                flashEffect.gameObject.AddComponent<GraphicRaycaster>();
        }

        if (highlightFrame != null)
        {
            // Highlight frame va al frente
            Canvas frameCanvas = highlightFrame.gameObject.GetComponent<Canvas>();
            if (frameCanvas == null) 
                frameCanvas = highlightFrame.gameObject.AddComponent<Canvas>();
            
            frameCanvas.overrideSorting = true;
            frameCanvas.sortingOrder = 10; // Muy adelante
        }
    }

    void InitializeElements()
    {
        if (highlightFrame != null)
        {
            highlightFrame.color = frameNormalColor;
            highlightFrame.gameObject.SetActive(true);
        }
        
        if (flashEffect != null)
        {
            flashEffect.color = flashNormalColor;
            flashEffect.gameObject.SetActive(true);
        }
        
        if (nameText != null)
        {
            nameText.color = textNormalColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) { }
    public void OnPointerExit(PointerEventData eventData) { }
    public void OnSelect(BaseEventData eventData) { }
    public void OnDeselect(BaseEventData eventData) { }

    public void SetHighlight(bool active)
    {
        if (active)
        {
            isHighlighted = true;
            if (currentAnimation != null)
                StopCoroutine(currentAnimation);
            currentAnimation = StartCoroutine(HighlightAnimation());
        }
        else
        {
            isHighlighted = false;
            if (currentAnimation != null)
                StopCoroutine(currentAnimation);
            currentAnimation = StartCoroutine(UnhighlightAnimation());
        }
    }

    private IEnumerator HighlightAnimation()
    {
        float elapsed = 0f;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            
            if (highlightFrame != null)
                highlightFrame.color = Color.Lerp(frameNormalColor, frameHighlightColor, t);
            
            if (flashEffect != null)
                flashEffect.color = Color.Lerp(flashNormalColor, flashHighlightColor, t);
            
            if (nameText != null)
                nameText.color = Color.Lerp(textNormalColor, textHighlightColor, t);
            
            yield return null;
        }

        if (flashEffect != null && isHighlighted)
        {
            Image flash = flashEffect;
            Color baseFlashColor = flashHighlightColor;
            
            while (isHighlighted && flash != null)
            {
                float pulse = (Mathf.Sin(Time.time * flashSpeed) + 1f) * 0.5f;
                float alpha = Mathf.Lerp(baseFlashColor.a * 0.8f, baseFlashColor.a * flashIntensity, pulse);
                flash.color = new Color(baseFlashColor.r, baseFlashColor.g, baseFlashColor.b, alpha);
                yield return null;
            }
        }
    }

    private IEnumerator UnhighlightAnimation()
    {
        float elapsed = 0f;
        Color startFrameColor = highlightFrame != null ? highlightFrame.color : frameNormalColor;
        Color startFlashColor = flashEffect != null ? flashEffect.color : flashNormalColor;
        Color startTextColor = nameText != null ? nameText.color : textNormalColor;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            
            if (highlightFrame != null)
                highlightFrame.color = Color.Lerp(startFrameColor, frameNormalColor, t);
            
            if (flashEffect != null)
                flashEffect.color = Color.Lerp(startFlashColor, flashNormalColor, t);
            
            if (nameText != null)
                nameText.color = Color.Lerp(startTextColor, textNormalColor, t);
            
            yield return null;
        }
        
        if (highlightFrame != null)
            highlightFrame.color = frameNormalColor;
        if (flashEffect != null)
            flashEffect.color = flashNormalColor;
        if (nameText != null)
            nameText.color = textNormalColor;
    }

    void OnDisable()
    {
        isHighlighted = false;
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }
        
        if (highlightFrame != null)
            highlightFrame.color = frameNormalColor;
        if (flashEffect != null)
            flashEffect.color = flashNormalColor;
        if (nameText != null)
            nameText.color = textNormalColor;
    }
}