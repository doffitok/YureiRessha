using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameLogoSplash : MonoBehaviour
{
    public CanvasGroup logoGroup;     // El CanvasGroup del GameLogo
    public RectTransform tagImage;    // La etiqueta
    public float fadeTime = 1f;
    public float moveAmount = 10f;
    public float moveSpeed = 1f;

    void Start()
    {
        logoGroup.alpha = 0f; // empieza invisible
        StartCoroutine(FadeInLogo());
    }

    IEnumerator FadeInLogo()
    {
        float t = 0f;
        while(t < fadeTime)
        {
            t += Time.deltaTime;
            logoGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeTime);
            yield return null;
        }
        logoGroup.alpha = 1f;

        // después de hacer fade in, comienza animación leve de la etiqueta
        StartCoroutine(WobbleTag());
    }

    IEnumerator WobbleTag()
    {
        Vector2 startPos = tagImage.anchoredPosition;
        while(true)
        {
            float yOffset = Mathf.Sin(Time.time * moveSpeed) * moveAmount;
            tagImage.anchoredPosition = startPos + new Vector2(0, yOffset);
            yield return null;
        }
    }
}
