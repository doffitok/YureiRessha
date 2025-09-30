using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class FoldoutController : MonoBehaviour
{
    public UIDocument uiDocument;

    void OnEnable()
    {
        // If uiDocument is not set in the Inspector, get it from the GameObject
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        var root = uiDocument.rootVisualElement;

    
        var myFoldout = root.Q<Foldout>("Foldout");
        if (myFoldout != null)
        {
            myFoldout.value = false;
        }
    }
}
