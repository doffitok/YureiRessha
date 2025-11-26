using UnityEngine;

[System.Serializable]
public class PassengerVisuals
{
    [Header("Animación y Apariencia")]
    public RuntimeAnimatorController animatorController;
    public Material characterMaterial;
    public Vector2 textureTiling = Vector2.one;
    public Vector2 textureOffset = Vector2.zero;
    
    [Header("Configuración del Plano")]
    public Vector2 planeSize = new Vector2(1f, 1f);
    public bool useBillboard = true;
}

public class PassengerData : MonoBehaviour
{
    [Header("Datos")]
    public string ID;
    public string nombre;

    [Header("Variables")]
    [Range(0, 20)] public int rating;
    [Range(0, 20)] public int dinero;

    [Header("Emociones - Amistad")]
    [Range(-10, 10)] public int amistad = 0;

    [Header("Demanda")]
    [Range(0, 100)] public int demandaMin = 1;
    [Range(0, 100)] public int demandaMax = 100;
    [Range(0, 20)] public int exigencia;

    [Header("Apariencia Visual")]
    public PassengerVisuals visuals = new PassengerVisuals();

    [Header("Debug")]
    public Color debugColor;
    public Texture2D debugTexture;

    [Header("Notas")]
    [TextArea(10, 10)]
    public string notas;

    public EstadoEmocional EstadoActual
    {
        get
        {
            if (amistad >= 5) return EstadoEmocional.Feliz;
            if (amistad <= -5) return EstadoEmocional.Enojado;
            return EstadoEmocional.Neutro;
        }
    }

    void Start()
    {
        ApplyCharacterAppearance();
    }

    public void CambiarAmistad(int cantidad)
    {
        amistad = Mathf.Clamp(amistad + cantidad, -10, 10);
        UpdateAnimationBasedOnEmotion();
    }

    private void ApplyCharacterAppearance()
    {
        // Aplicar animator
        Animator animator = GetComponent<Animator>();
        if (animator != null && visuals.animatorController != null)
        {
            animator.runtimeAnimatorController = visuals.animatorController;
        }

        // Aplicar material/textura
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            if (visuals.characterMaterial != null)
            {
                renderer.material = visuals.characterMaterial;
            }
            else if (debugTexture != null)
            {
                renderer.material.mainTexture = debugTexture;
            }
            
            renderer.material.mainTextureScale = visuals.textureTiling;
            renderer.material.mainTextureOffset = visuals.textureOffset;
            
            if (debugColor != Color.white)
            {
                renderer.material.color = debugColor;
            }
        }

        // Configurar escala del plano
        if (visuals.planeSize != Vector2.one)
        {
            Transform childTransform = GetComponentInChildren<Renderer>()?.transform;
            if (childTransform != null)
            {
                childTransform.localScale = new Vector3(visuals.planeSize.x, visuals.planeSize.y, 1f);
            }
        }

        // Añadir billboard si es necesario
        if (visuals.useBillboard && GetComponent<BillboardFixedX>() == null)
        {
            gameObject.AddComponent<BillboardFixedX>();
        }
    }

    private void UpdateAnimationBasedOnEmotion()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetInteger("EmotionState", (int)EstadoActual);
        }
    }

    public void SetAnimation(RuntimeAnimatorController newAnimator)
    {
        visuals.animatorController = newAnimator;
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.runtimeAnimatorController = newAnimator;
        }
    }

    public void SetMaterial(Material newMaterial)
    {
        visuals.characterMaterial = newMaterial;
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material = newMaterial;
        }
    }
}

public enum EstadoEmocional
{
    Enojado,
    Neutro,
    Feliz
}