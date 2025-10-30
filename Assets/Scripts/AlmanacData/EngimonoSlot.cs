using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EngimonoSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int index; // 0, 1 o 2 según el engimono
    private AlmanacController controller;

    public void Setup(int slotIndex, AlmanacController almaController)
    {
        index = slotIndex;
        controller = almaController;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        controller?.ShowEngimonoInfo(index);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        controller?.HideEngimonoInfo();
    }
}
