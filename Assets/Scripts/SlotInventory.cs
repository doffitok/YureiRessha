using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotInventory : MonoBehaviour
{
    public TextMesh quantitytext;
    public Image iconItem;

    public void SetInfo(Sprite itemSprite, int quantity)
    {
        iconItem.useSpriteMesh = itemSprite;
        quantitytext.text = "x" + quantity;
    }

}
