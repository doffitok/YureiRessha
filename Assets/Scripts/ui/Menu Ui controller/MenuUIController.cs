using UnityEngine;
using UnityEngine.UI;

public class MenuUIController : MonoBehaviour
{
    [Header("Main Menu Elements")]
    public GameObject logo;
    public GameObject[] mainButtons;  // Asigna tus botones (Play, Credits, Exit, etc.)

    [Header("Credits Panel")]
    public GameObject creditsPanel;   // Panel que se mostrará al presionar "Credits"
    public Button backButton;         // Botón de salida dentro de los créditos

    void Start()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(false);

        if (backButton != null)
            backButton.onClick.AddListener(BackToMenu);
    }

    public void ShowCredits()
    {
        // Oculta el logo y los botones principales
        if (logo != null) logo.SetActive(false);

        foreach (GameObject btn in mainButtons)
            btn.SetActive(false);

        // Muestra el panel de créditos
        if (creditsPanel != null)
            creditsPanel.SetActive(true);
    }

    public void BackToMenu()
    {
        // Oculta los créditos
        if (creditsPanel != null)
            creditsPanel.SetActive(false);

        // Vuelve a mostrar el logo y los botones principales
        if (logo != null) logo.SetActive(true);

        foreach (GameObject btn in mainButtons)
            btn.SetActive(true);
    }
}
