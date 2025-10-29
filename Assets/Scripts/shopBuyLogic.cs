using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class shopBuyLogic : MonoBehaviour, IPointerClickHandler
{
    [Header("Referencias")]
    [SerializeField] private GameStats gameStats;

    private uiEngimonosInventoryManager inventory;
    private uiEngimonoShopInfo shopInfo;
    private ShopItemMarker marker;

    private void Awake()
    {
        shopInfo = GetComponent<uiEngimonoShopInfo>();
        marker   = GetComponent<ShopItemMarker>();
    }

    private void Start()
    {
        if (gameStats == null)
            gameStats = FindObjectOfType<GameStats>();

        inventory = uiEngimonosInventoryManager.Instance;

        if (marker == null)
            Debug.LogWarning("[shopBuyLogic] Este objeto NO tiene ShopItemMarker; no se podrá comprar (correcto para inventario).");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        IntentarCompra();
    }

    private void IntentarCompra()
    {
        // Bloquear compra si NO es un item de tienda
        if (marker == null) return;

        if (gameStats == null || inventory == null || shopInfo == null || shopInfo.engimonoData == null)
            return;

        int precio = shopInfo.engimonoData.Compra;
        if (gameStats.GetDineroTotal() < precio)
        {
            Debug.Log("Dinero insuficiente.");
            return;
        }

        // Restar dinero
        gameStats.SpendMoney(precio);

        // (Opcional) limpiar elementos visuales de tienda antes de destruir
        shopInfo.BorrarDecorTienda();

        // Crear instancia runtime y agregar al inventario
        var inst = new EngimonoInstance { data = shopInfo.engimonoData, comprado = true };
        inventory.AddEngimono(inst);

        // Destruir el objeto de TIENDA (este mismo)
        Destroy(gameObject);
    }
}