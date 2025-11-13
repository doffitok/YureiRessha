using UnityEngine;
using UnityEngine.UI;

public class ForceBackgroundOrder : MonoBehaviour
{
    public Image targetImage;
    public bool setAsBackground = true;
    
    void Start()
    {
        if (targetImage != null && setAsBackground)
        {
            targetImage.transform.SetAsFirstSibling();
            
            // Forzar update
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }
    }
    
    void OnEnable()
    {
        if (targetImage != null && setAsBackground)
        {
            targetImage.transform.SetAsFirstSibling();
        }
    }
}