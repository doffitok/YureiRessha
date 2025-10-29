using UnityEngine;

public class InventorySlotUI : MonoBehaviour
{
    public InventoryItemUI currentItem = null;

    // Indica si este slot está libre
    public bool IsFree()
    {
        return currentItem == null;
    }

    // Marca que een este slot se colocó un item
    public void SetItem(InventoryItemUI item)
    {
        currentItem = item;
        item.currentSlot = this;
    }

    // Limpia el slot
    public void Clear()
    {
        currentItem = null;
    }
}