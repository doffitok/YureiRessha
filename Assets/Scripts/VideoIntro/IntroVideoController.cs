using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroVideoController : MonoBehaviour
{
    [Header("Referencias UI")]
    public Button botonStart;
    public GameObject panelConfirmacion;
    public GameObject panelVideo;
    public VideoPlayer videoPlayer;
    public string escenaSiguiente;

    [Header("Canvas Groups")]
    public CanvasGroup mainMenuGroup;   // ← botones del menú principal
    public CanvasGroup confirmGroup;    // ← panel de confirmación

    [Header("Audio")]
    public AudioSource musicaMenu;      // ← arrastra aquí tu música del menú
    public float volumenObjetivo = 0.1f; // volumen al que baja
    public float duracionFadeMusica = 1.5f; // segundos del fade

    private Vector3 panelVideoPosInicial;

    void Start()
    {
        panelConfirmacion.SetActive(false);
        panelVideo.SetActive(false);

        panelVideoPosInicial = panelVideo.transform.localPosition;

        if (botonStart != null)
            botonStart.onClick.AddListener(MostrarConfirmacion);

        // Asegura estados iniciales
        if (mainMenuGroup != null) mainMenuGroup.alpha = 1;
        if (confirmGroup != null)
        {
            confirmGroup.alpha = 0;
            confirmGroup.interactable = false;
            confirmGroup.blocksRaycasts = false;
        }
    }

    public void MostrarConfirmacion()
    {
        Debug.Log("🎬 Botón Start presionado — iniciando transición");

        // Desactiva botones del menú con fade out
        if (mainMenuGroup != null)
            StartCoroutine(FadeCanvasGroup(mainMenuGroup, 1f, 0f, 0.4f, false));

        // Activa panel de confirmación con fade in
        panelConfirmacion.SetActive(true);
        if (confirmGroup != null)
        {
            confirmGroup.interactable = true;
            confirmGroup.blocksRaycasts = true;
            StartCoroutine(FadeCanvasGroup(confirmGroup, 0f, 1f, 0.6f, true));
        }
    }

    public void ElegirVerVideo(bool verVideo)
    {
        if (verVideo)
        {
            Debug.Log("📽️ El jugador eligió ver el video");

            // 🔊 Fade out de la música
            if (musicaMenu != null)
                StartCoroutine(FadeOutMusica());

            panelConfirmacion.SetActive(false);
            panelVideo.SetActive(true);
            StartCoroutine(BajarPanelYReproducir());
        }
        else
        {
            Debug.Log("⏭️ El jugador eligió saltar el video");
            panelConfirmacion.SetActive(false);
            CargarSiguienteEscena();
        }
    }

    IEnumerator BajarPanelYReproducir()
    {
        Vector3 destino = panelVideoPosInicial;

        while (Vector3.Distance(panelVideo.transform.localPosition, destino) > 0.1f)
        {
            panelVideo.transform.localPosition = Vector3.Lerp(panelVideo.transform.localPosition, destino, Time.deltaTime * 3f);
            yield return null;
        }

        videoPlayer.targetTexture.Release();
        yield return null;

        videoPlayer.Play();
        videoPlayer.loopPointReached += _ => StartCoroutine(EsperarYSalir());
    }

    IEnumerator EsperarYSalir()
    {
        Debug.Log("🎞️ Video terminado. Esperando antes de cambiar de escena...");
        yield return new WaitForSeconds(0.5f);
        videoPlayer.targetTexture.Release();
        yield return null;
        CargarSiguienteEscena();
    }

    void CargarSiguienteEscena()
    {
        Debug.Log("🚉 Cargando escena siguiente: " + escenaSiguiente);
        SceneManager.LoadScene(escenaSiguiente);
    }

    // 🔹 Corrutina reutilizable para fades
    IEnumerator FadeCanvasGroup(CanvasGroup group, float start, float end, float duration, bool interactableAfter)
    {
        float t = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        while (t < duration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(start, end, t / duration);
            yield return null;
        }

        group.alpha = end;
        group.interactable = interactableAfter;
        group.blocksRaycasts = interactableAfter;
    }

    // 🔊 Corrutina para hacer fade out de la música
    IEnumerator FadeOutMusica()
    {
        float inicioVolumen = musicaMenu.volume;
        float t = 0f;

        while (t < duracionFadeMusica)
        {
            t += Time.deltaTime;
            musicaMenu.volume = Mathf.Lerp(inicioVolumen, volumenObjetivo, t / duracionFadeMusica);
            yield return null;
        }

        musicaMenu.volume = volumenObjetivo;
    }
}
