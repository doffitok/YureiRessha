using UnityEngine;
using UnityEngine.UI;

public class uiInventorySlotState : MonoBehaviour
{
    public bool isOccupied = false;
    private Image slotImage;

    private void Awake()
    {
        slotImage = GetComponent<Image>();
        if (slotImage == null)
            Debug.LogWarning($"{name} no tiene componente Image.");
    }

    public void SetColor(Color color)
    {
        if (slotImage != null)
            slotImage.color = color;
    }

    public void SetState(bool occupied)
    {
        isOccupied = occupied;
        SetColor(occupied ? Color.red : Color.white);
    }
}