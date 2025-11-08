using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class SNESEasterEgg : MonoBehaviour
{
    [Header("🎵 Referencias")]
    public MusicController musicController;
    public AudioClip temaSNES;
    public AudioClip temaVictoria;
    public AudioClip sonidoTecla;

    [Header("⚙️ Configuración")]
    public float delayAntesDeSNES = 0.5f;

    private string secuencia = "SNES";
    private string inputActual = "";
    private bool activado = false;

    void Start()
    {
        if (musicController == null)
        {
            musicController = FindObjectOfType<MusicController>();
            if (musicController == null)
                Debug.LogWarning("⚠️ No se encontró MusicController en la escena.");
        }
    }

    void Update()
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            return;

        if (activado || Keyboard.current == null)
            return;

        // 🔹 Detecta teclas individuales
        if (Keyboard.current.sKey.wasPressedThisFrame) RegistrarTecla('S');
        else if (Keyboard.current.nKey.wasPressedThisFrame) RegistrarTecla('N');
        else if (Keyboard.current.eKey.wasPressedThisFrame) RegistrarTecla('E');
        else if (Keyboard.current.anyKey.wasPressedThisFrame)
            ReiniciarSecuencia();
    }

    void RegistrarTecla(char tecla)
    {
        if (Camera.main != null && sonidoTecla != null)
            AudioSource.PlayClipAtPoint(sonidoTecla, Camera.main.transform.position);

        inputActual += tecla;

        // Si se excede la longitud, se reinicia
        if (inputActual.Length > secuencia.Length)
        {
            ReiniciarSecuencia();
            return;
        }

        // Coincide con el patrón hasta ahora
        if (secuencia.StartsWith(inputActual))
        {
            if (inputActual == secuencia)
            {
                Debug.Log("🎉 Secuencia SNES completada");
                StartCoroutine(ReproducirSecuencia());
                ReiniciarSecuencia();
            }
        }
        else
        {
            ReiniciarSecuencia();
        }
    }

    void ReiniciarSecuencia() => inputActual = "";

    IEnumerator ReproducirSecuencia()
    {
        activado = true;

        if (musicController == null)
        {
            Debug.LogError("🚫 No hay MusicController asignado, no puedo reproducir música.");
            yield break;
        }

        // 🏆 Tema de victoria
        if (temaVictoria != null)
        {
            musicController.CambiarCancion(temaVictoria, false);
            yield return new WaitForSeconds(temaVictoria.length + delayAntesDeSNES);
        }

        // 🎮 Tema SNES
        if (temaSNES != null)
        {
            musicController.CambiarCancion(temaSNES, true);
        }

        Debug.Log("✨ Código SNES activado — nueva música en reproducción");
        activado = false;
    }
}
