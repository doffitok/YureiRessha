using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeInEffect : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 2f;

    void Start()
    {
        if (fadeImage != null)
            StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        Color c = fadeImage.color;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(1, 0, t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
        c.a = 0;
        fadeImage.color = c;
    }
}
