using UnityEngine;

public class PassengerAnimationInjector : MonoBehaviour
{
    [System.Serializable]
    public class CharacterAnimationSet
    {
        public string characterID;
        public RuntimeAnimatorController animatorController;
        public AnimationClip idleAnimation;
        public AnimationClip talkAnimation;
    }

    [Header("Animation Sets")]
    public CharacterAnimationSet[] animationSets;

    [Header("References")]
    public Animator characterAnimator;
    
    private PassengerData passengerData;

    void Start()
    {
        passengerData = GetComponent<PassengerData>();
        characterAnimator = GetComponentInChildren<Animator>();
        
        if (characterAnimator == null)
        {
            // Crear Animator si no existe
            GameObject quad = GetComponentInChildren<MeshRenderer>()?.gameObject;
            if (quad != null)
            {
                characterAnimator = quad.AddComponent<Animator>();
            }
        }

        ApplyCharacterAnimation();
    }

    public void ApplyCharacterAnimation()
    {
        if (passengerData == null || characterAnimator == null) return;

        // Buscar el set de animación que coincida con el ID del personaje
        foreach (var animationSet in animationSets)
        {
            if (animationSet.characterID == passengerData.ID)
            {
                characterAnimator.runtimeAnimatorController = animationSet.animatorController;
                Debug.Log($"🎬 Animación aplicada: {animationSet.characterID}");
                return;
            }
        }

        // Fallback: usar el primer set disponible
        if (animationSets.Length > 0 && animationSets[0].animatorController != null)
        {
            characterAnimator.runtimeAnimatorController = animationSets[0].animatorController;
            Debug.Log("🎬 Animación por defecto aplicada");
        }
    }
}