using UnityEngine;

public class MusicController : MonoBehaviour
{
    [Header("🎵 Referencias de audio")]
    public AudioSource musicaSource;
    public AudioClip musicaInicial;

    void Awake()
    {
        // Evita duplicados
        var musicas = FindObjectsOfType<MusicController>();
        if (musicas.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        // Inicia la música si no está sonando
        if (musicaSource != null && musicaInicial != null)
        {
            musicaSource.clip = musicaInicial;
            musicaSource.loop = true;
            if (!musicaSource.isPlaying)
                musicaSource.Play();
        }
    }

    public void CambiarCancion(AudioClip nuevaCancion, bool loop = true)
    {
        if (musicaSource == null || nuevaCancion == null)
            return;

        musicaSource.Stop();
        musicaSource.clip = nuevaCancion;
        musicaSource.loop = loop;
        musicaSource.Play();
    }

    public void DetenerMusica()
    {
        if (musicaSource != null)
            musicaSource.Stop();
    }

    public bool EstaReproduciendo()
    {
        return musicaSource != null && musicaSource.isPlaying;
    }
}
