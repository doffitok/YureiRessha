using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    [Header("🔹 Imagen del PNG que se moverá")]
    public RectTransform transitionImage;

    [Header("🔹 Parámetros de movimiento")]
    public Vector2 startPos = new Vector2(-2000f, 0f); // fuera de pantalla (izquierda)
    public Vector2 endPos = new Vector2(2000f, 0f);   // fuera de pantalla (derecha)
    public float moveDuration = 1.2f;                 // tiempo de cruce

    [Header("🔹 Parámetros de fade")]
    public Image fadeImage;       // imagen negra/blanca encima (fade out)
    public float fadeDuration = 0.8f;

    [Header("🔹 Control general")]
    public bool autoPlay = false; // para test
    private bool isTransitioning = false;

    void Start()
    {
        if (fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, 0); // transparente

        if (transitionImage != null)
            transitionImage.anchoredPosition = startPos;

        if (autoPlay)
            StartCoroutine(PlayTransition("NextSceneName"));
    }

    public void Play(string sceneName)
    {
        if (!isTransitioning)
            StartCoroutine(PlayTransition(sceneName));
    }

    IEnumerator PlayTransition(string sceneName)
    {
        isTransitioning = true;

        // 🔹 Aseguramos que el PNG sea visible e interactúe bien
        transitionImage.gameObject.SetActive(true);
        var img = transitionImage.GetComponent<Image>();
        if (img != null) img.raycastTarget = false; // evita bloquear clics

        // 🔹 Movimiento principal (PNG cruza la pantalla)
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / moveDuration);
            transitionImage.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        transitionImage.anchoredPosition = endPos;

        // 🔹 Fade out + cambio de escena
        if (fadeImage != null)
        {
            float fadeElapsed = 0f;
            while (fadeElapsed < fadeDuration)
            {
                fadeElapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0, 1, fadeElapsed / fadeDuration);
                fadeImage.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
        }

        // 🔹 Cambio de escena
        SceneManager.LoadScene(sceneName);

        // 🔹 Esperar un frame antes de permitir salida (importante)
        yield return null;

        // 🔹 Mover rápidamente el PNG fuera de pantalla y desactivar
        if (transitionImage != null)
        {
            transitionImage.anchoredPosition = startPos;
            transitionImage.gameObject.SetActive(false);
        }

        isTransitioning = false;
    }
}
