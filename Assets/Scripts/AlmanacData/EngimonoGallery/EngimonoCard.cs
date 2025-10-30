using UnityEngine;
using UnityEngine.UI;

public class EngimonoCard : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image iconImage;        // Imagen del Engimono en el ScrollView
    public Image backgroundImage;  // Fondo de la tarjeta (opcional)

    public void SetEngimono(EngimonoData data)
    {
        // Asignar el icono principal (miniatura del ScrollView)
        if (iconImage != null)
            iconImage.sprite = data.engimonoIcon != null ? data.engimonoIcon : null;

        // Fondo opcional (sprite o color)
        if (backgroundImage != null)
        {
            if (data.backgroundSprite != null)
                backgroundImage.sprite = data.backgroundSprite;
            else
                backgroundImage.color = data.backgroundColor;
        }
    }
}
