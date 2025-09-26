using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    private UIDocument uiDocument;

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("No se encontró UIDocument en el GameObject. Añade un UIDocument con el UXML que contiene los botones.");
            return;
        }

        var root = uiDocument.rootVisualElement;

        // Jugar al juego
        var changeSceneButton = root.Q<Button>("changeSceneButton");
        if (changeSceneButton != null)
        {
            changeSceneButton.clicked += () => SceneManager.LoadScene("Tren");
        }
        else Debug.LogWarning("No se encontró el botón 'changeSceneButton'.");

        // Almanaque de personajes
        var changeAlmanaque = root.Q<Button>("changealmanaque");
        if (changeAlmanaque != null)
        {
            changeAlmanaque.clicked += () => ShowOptions();
        }
        else Debug.LogWarning("No se encontró el botón 'changealmanaque'.");

        // Salir
        var changeQuit = root.Q<Button>("changeQuit");
        if (changeQuit != null)
        {
            changeQuit.clicked += () => QuitGame();
        }
        else Debug.LogWarning("No se encontró el botón 'changeQuit'.");

        // Tutorial (asegúrate que en tu UXML el botón tenga nombre/id "tutorial")
        var tutorial = root.Q<Button>("tutorial");
        if (tutorial != null)
        {
            tutorial.clicked += () => SceneManager.LoadScene("Tutorial");
        }
        else Debug.LogWarning("No se encontró el botón 'tutorial'.");
    }

    void ShowOptions()
    {
        Debug.Log("Mostrar opciones (no implementado)");
        // Aquí puedes activar un nuevo panel con más configuraciones
    }

    void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnDisable()
    {
        // Opcional: quitar listeners para evitar duplicados si el objeto se habilita/deshabilita varias veces
        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;

        var changeSceneButton = root.Q<Button>("changeSceneButton");
        if (changeSceneButton != null) changeSceneButton.clicked -= () => SceneManager.LoadScene("Tren");

        var changeAlmanaque = root.Q<Button>("changealmanaque");
        if (changeAlmanaque != null) changeAlmanaque.clicked -= () => ShowOptions();

        var changeQuit = root.Q<Button>("changeQuit");
        if (changeQuit != null) changeQuit.clicked -= () => QuitGame();

        var tutorial = root.Q<Button>("tutorial");
        if (tutorial != null) tutorial.clicked -= () => SceneManager.LoadScene("Tutorial");
    }
}
