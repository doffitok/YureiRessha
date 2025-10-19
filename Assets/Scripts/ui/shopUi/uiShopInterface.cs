using UnityEngine;
using UnityEngine.EventSystems;

public class uiShopInterface : MonoBehaviour, IPointerClickHandler
{
    [Header("Referencias de UI")]
    public RectTransform shopBox;       // Panel principal de la tienda
    public RectTransform shopBoxHide;   // Objeto que cierra la tienda

    [Header("Configuración")]
    public float moveDistance = -280f;  // Distancia en X
    public float moveSpeed = 10f;       // Velocidad del movimiento suave

    private Vector2 closedPos;
    private Vector2 openPos;
    private bool isOpen = false;

    void Start()
    {
        if (shopBox == null || shopBoxHide == null)
        {
            Debug.LogError("Asigna shopBox y shopBoxHide.");
            return;
        }

        // Posición cerrada inicial
        closedPos = shopBox.anchoredPosition;
        // Posición abierta (mover en X)
        openPos = closedPos + new Vector2(moveDistance, 0f);
    }

    void Update()
    {
        if (shopBox == null) return;

        // Movimiento suave hacia la posición deseada
        Vector2 targetPos = isOpen ? openPos : closedPos;
        shopBox.anchoredPosition = Vector2.Lerp(shopBox.anchoredPosition, targetPos, Time.deltaTime * moveSpeed);
    }

    /// <summary>
    /// Se llama al cliclear el shopBox
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // Si clickeaste el shopBoxHide, cerrar
        if (eventData.pointerEnter == shopBoxHide.gameObject)
        {
            isOpen = false;
        }
        else if (eventData.pointerEnter == shopBox.gameObject)
        {
            // Si clickeaste el shopBox mismo, abrir
            isOpen = true;
        }
    }
}