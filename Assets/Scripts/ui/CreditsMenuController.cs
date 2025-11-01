using UnityEngine;
using System.Collections;

public class CreditsMenuController : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject creditsPanel;
    public GameObject buttonsPanel;
    public GameObject logo;
    public float fadeDuration = 1f;

    private CanvasGroup creditsCanvasGroup;

    void Start()
    {
        if (creditsPanel != null)
        {
            creditsCanvasGroup = creditsPanel.GetComponent<CanvasGroup>();
            if (creditsCanvasGroup == null)
                creditsCanvasGroup = creditsPanel.AddComponent<CanvasGroup>();

            creditsPanel.SetActive(false);
            creditsCanvasGroup.alpha = 0f;
            creditsCanvasGroup.interactable = false;
            creditsCanvasGroup.blocksRaycasts = false;
        }
    }

    public void ShowCredits()
    {
        if (creditsPanel == null) return;

        buttonsPanel.SetActive(false);
        logo.SetActive(false);

        creditsPanel.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(FadeCanvasGroup(creditsCanvasGroup, 0f, 1f, fadeDuration, true));
    }

    public void HideCredits()
    {
        if (creditsPanel == null) return;

        StopAllCoroutines();
        StartCoroutine(FadeCanvasGroup(creditsCanvasGroup, 1f, 0f, fadeDuration, false, () =>
        {
            creditsPanel.SetActive(false);
            buttonsPanel.SetActive(true);
            logo.SetActive(true);
        }));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration, bool makeInteractable, System.Action onEnd = null)
    {
        float time = 0f;
        cg.alpha = from;

        cg.interactable = makeInteractable;
        cg.blocksRaycasts = makeInteractable;

        while (time < duration)
        {
            cg.alpha = Mathf.Lerp(from, to, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        cg.alpha = to;

        if (onEnd != null)
            onEnd();
    }
}
