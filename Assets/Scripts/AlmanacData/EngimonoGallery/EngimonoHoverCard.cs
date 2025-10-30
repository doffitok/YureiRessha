using UnityEngine;
using UnityEngine.EventSystems;

public class EngimonoHoverCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private EngimonoGalleryController galleryController;
    private EngimonoData engimonoData;

    // Configuración inicial desde el controlador
    public void Setup(EngimonoGalleryController controller, EngimonoData data)
    {
        galleryController = controller;
        engimonoData = data;
    }

    // Cuando el mouse entra en el EngimonoCard
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (galleryController != null && engimonoData != null)
        {
            // Solo pasamos el EngimonoData
            galleryController.ShowEngimono(engimonoData);
        }
    }

    // Cuando el mouse sale del EngimonoCard
    public void OnPointerExit(PointerEventData eventData)
    {
        if (galleryController != null)
        {
            galleryController.HideEngimonoPanel();
        }
    }
}
