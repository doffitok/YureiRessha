using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenuAnimator : MonoBehaviour
{
    [Header("Botones del menú (en orden)")]
    public RectTransform[] menuButtons; // Asigna los botones aquí
    public string[] sceneNames;         // Nombre de las escenas correspondientes a cada botón

    [Header("Audio SFX")]
    public AudioSource audioSource;     // Un solo AudioSource general
    public AudioClip buttonClickSFX;    // SFX al hacer click en cualquier botón

    [Header("Ajustes de animación")]
    public float spacing = 150f; 
    public float animationTime = 0.8f; 
    public float delayBetweenButtons = 0.1f;

    [Header("Posiciones")]
    public Vector2 startOffset = new Vector2(-800f, 0f);
    public Vector2 basePosition = new Vector2(0f, 100f);

    void Start()
    {
        StartCoroutine(AnimateMenu());
    }

    IEnumerator AnimateMenu()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            RectTransform btn = menuButtons[i];
            Vector2 targetPos = basePosition + new Vector2(0f, -i * spacing);
            Vector2 startPos = targetPos + startOffset;

            btn.anchoredPosition = startPos;
            btn.gameObject.SetActive(true);

            float elapsed = 0f;
            while (elapsed < animationTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / animationTime);
                btn.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                yield return null;
            }

            btn.anchoredPosition = targetPos;

            // 🔹 Asignar funcionalidad del botón
            Button buttonComp = btn.GetComponent<Button>();
            if (buttonComp != null)
            {
                int index = i; // importante para closures
                buttonComp.onClick.RemoveAllListeners();
                buttonComp.onClick.AddListener(() =>
                {
                    // Reproducir SFX
                    if (audioSource != null && buttonClickSFX != null)
                        audioSource.PlayOneShot(buttonClickSFX);

                    // Cambiar escena o salir después de un pequeño delay
                    if (sceneNames != null && index < sceneNames.Length)
                        StartCoroutine(LoadSceneAfterDelay(sceneNames[index], 0.2f)); // 0.2s de espera
                });
            }

            yield return new WaitForSeconds(delayBetweenButtons);
        }

        // Cuando termina todo, inicia la animación flotante
        StartCoroutine(IdleFloat());
    }

    IEnumerator IdleFloat()
    {
        while (true)
        {
            foreach (RectTransform btn in menuButtons)
            {
                float yOffset = Mathf.Sin(Time.time * 1.5f + btn.GetInstanceID()) * 3f;
                btn.anchoredPosition += new Vector2(0f, yOffset * Time.deltaTime);
            }
            yield return null;
        }
    }

    IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (sceneName == "EXIT")
            Application.Quit();
        else
            SceneManager.LoadScene(sceneName);
    }
}
