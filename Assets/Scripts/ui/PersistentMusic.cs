using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentMusic : MonoBehaviour
{
    public string[] allowedScenes; // Escenas donde debe sonar la música
    private static PersistentMusic instance;

    void Awake()
    {
        // Evita duplicados
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // Mantiene el objeto al cambiar de escena
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        CheckScene(SceneManager.GetActiveScene().name);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckScene(scene.name);
    }

    void CheckScene(string sceneName)
    {
        bool shouldPlay = false;
        foreach (var s in allowedScenes)
        {
            if (s == sceneName)
            {
                shouldPlay = true;
                break;
            }
        }

        gameObject.SetActive(shouldPlay);
    }
}
