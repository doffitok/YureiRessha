using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LogoSplash : MonoBehaviour
{
    public CanvasGroup logoGroup;
    public RectTransform tagImage;
    public float fadeTime = 1f;
    public float moveAmount = 10f;
    public float moveSpeed = 1f;

    private bool isWobbling = false;

    void Start()
    {
        logoGroup.alpha = 0f;
        StartCoroutine(FadeInLogo());
    }

    IEnumerator FadeInLogo()
    {
        float t = 0f;
        Vector2 startPos = tagImage.anchoredPosition;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            logoGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeTime);

            float yOffset = Mathf.Sin(Time.time * moveSpeed) * moveAmount * (t / fadeTime);
            tagImage.anchoredPosition = startPos + new Vector2(0, yOffset);

            yield return null;
        }

        logoGroup.alpha = 1f;
        StartCoroutine(WobbleTag());
    }

    IEnumerator WobbleTag()
    {
        isWobbling = true;
        Vector2 startPos = tagImage.anchoredPosition;

        while (isWobbling)
        {
            float yOffset = Mathf.Sin(Time.time * moveSpeed) * moveAmount;
            tagImage.anchoredPosition = startPos + new Vector2(0, yOffset);
            yield return null;
        }
    }

    public void RestartWobble()
    {
        StopAllCoroutines();
        StartCoroutine(WobbleTag());
    }

    public void StopWobble()
    {
        isWobbling = false;
    }
}
