using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    private UIDocument uiDocument;
    
    // Referencias a los botones para poder remover los eventos correctamente
    private Button changeSceneButton;
    private Button changeAlmanaque;
    private Button changeQuit;
    private Button tutorial;

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("No se encontró UIDocument en el GameObject. Añade un UIDocument con el UXML que contiene los botones.");
            return;
        }

        var root = uiDocument.rootVisualElement;
    }
    // Métodos separados para cada evento
    void OnChangeSceneButtonClicked()
    {
        SceneManager.LoadScene("Tren");
    }

    void OnChangeAlmanaqueClicked()
    {
        SceneManager.LoadScene("Fungu test");
    }

    void OnChangeQuitClicked()
    {
        QuitGame();
    }

    void OnTutorialClicked()
    {
        SceneManager.LoadScene("Tutorial");
    }

    void ShowOptions()
    {
        Debug.Log("Mostrar opciones (no implementado)");
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
        // Verificar que uiDocument y rootVisualElement no sean nulos
        if (uiDocument == null || uiDocument.rootVisualElement == null) 
            return;

        // Remover eventos usando los métodos nombrados
        if (changeSceneButton != null) 
            changeSceneButton.clicked -= OnChangeSceneButtonClicked;

        if (changeAlmanaque != null) 
            changeAlmanaque.clicked -= OnChangeAlmanaqueClicked;

        if (changeQuit != null) 
            changeQuit.clicked -= OnChangeQuitClicked;

        if (tutorial != null) 
            tutorial.clicked -= OnTutorialClicked;
    }

    void OnDestroy()
    {
        // También es buena práctica limpiar en OnDestroy
        OnDisable();
    }
}