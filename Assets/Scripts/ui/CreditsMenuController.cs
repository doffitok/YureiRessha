using UnityEngine;
using System.Collections;

public class CreditsMenuController : MonoBehaviour
{
    [Header("Referencias")]
    public CanvasGroup creditsGroup;
    public CanvasGroup buttonsGroup;
    public CanvasGroup logoGroup;

    [Header("Configuración")]
    public float fadeDuration = 1f;

    void Start()
    {
        // Aseguramos que el panel de créditos comience invisible
        if (creditsGroup != null)
        {
            creditsGroup.alpha = 0f;
            creditsGroup.interactable = false;
            creditsGroup.blocksRaycasts = false;
        }
    }

    public void ShowCredits()
    {
        StopAllCoroutines();

        // Fade OUT botones + logo
        if (buttonsGroup != null)
            StartCoroutine(FadeCanvasGroup(buttonsGroup, buttonsGroup.alpha, 0f, fadeDuration, false));

        if (logoGroup != null)
            StartCoroutine(FadeCanvasGroup(logoGroup, logoGroup.alpha, 0f, fadeDuration, false));

        // Fade IN créditos
        if (creditsGroup != null)
            StartCoroutine(FadeCanvasGroup(creditsGroup, creditsGroup.alpha, 1f, fadeDuration, true));
    }

    public void HideCredits()
    {
        StopAllCoroutines();

        // Fade OUT créditos
        if (creditsGroup != null)
            StartCoroutine(FadeCanvasGroup(creditsGroup, creditsGroup.alpha, 0f, fadeDuration, false));

        // Fade IN botones + logo
        if (buttonsGroup != null)
            StartCoroutine(FadeCanvasGroup(buttonsGroup, buttonsGroup.alpha, 1f, fadeDuration, true));

        if (logoGroup != null)
            StartCoroutine(FadeCanvasGroup(logoGroup, logoGroup.alpha, 1f, fadeDuration, true));

        // Reactivar wobble del logo
        var logoScript = logoGroup?.GetComponent<LogoSplash>();
        if (logoScript != null)
            logoScript.RestartWobble();
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration, bool interactable)
    {
        float time = 0f;
        cg.interactable = interactable;
        cg.blocksRaycasts = interactable;

        while (time < duration)
        {
            time += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, time / duration);
            yield return null;
        }

        cg.alpha = to;
    }
}
