using UnityEngine;
using UnityEngine.UI;

public class ToggleLeverButton : MonoBehaviour
{
    [Header("Sprites de la palanca")]
    public Sprite palancaArriba;
    public Sprite palancaAbajo;

    private Image image;
    private bool estaAbajo = false;

    private void Awake()
    {
        image = GetComponent<Image>();

        if (image != null && palancaArriba != null)
            image.sprite = palancaArriba;
    }

    public void OnClickPalanca()
    {
        estaAbajo = !estaAbajo;

        if (image == null) return;

        image.sprite = estaAbajo ? palancaAbajo : palancaArriba;
    }

    // Opcional: para otros scripts
    public bool EstaAbajo()
    {
        return estaAbajo;
    }
}
