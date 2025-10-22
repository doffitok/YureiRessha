using UnityEngine;
using UnityEngine.EventSystems;

// Este script permite comprar engimonos haciendo click sobre ellos
public class shopBuyLogic : MonoBehaviour, IPointerClickHandler
{
    [Header("Referencias")]
    [SerializeField] private GameStats gameStats; // Referencia al dinero del jugador

    private void Start()
    {
        // Buscar GameStats automáticamente si no se asignó
        if (gameStats == null)
        {
            gameStats = FindObjectOfType<GameStats>();
            if (gameStats == null)
            {
                Debug.LogError("[shopBuyLogic] No se encontró GameStats en la escena.");
            }
        }
    }

    // Este método se llama automáticamente cuando se hace click en el objeto (necesita EventSystem)
    public void OnPointerClick(PointerEventData eventData)
    {
        IntentarCompra();
    }

    private void IntentarCompra()
    {
        if (gameStats == null) return;

        // Buscar el componente uiEngimonoShopInfo en este objeto
        var shopInfo = GetComponent<uiEngimonoShopInfo>();
        if (shopInfo == null)
        {
            Debug.LogWarning($"[shopBuyLogic] No se encontró el componente uiEngimonoShopInfo en {gameObject.name}");
            return;
        }

        var engimono = shopInfo.engimonoData; // Aquí obtenemos el ScriptableObject directamente
        if (engimono == null)
        {
            Debug.LogWarning($"[shopBuyLogic] El campo engimonoData en {gameObject.name} es null");
            return;
        }

        int dineroActual = gameStats.GetDineroTotal();
        int precio = engimono.Compra;

        if (dineroActual >= precio)
        {
            // Restar dinero usando SpendMoney para afectar directamente el valor base
            gameStats.SpendMoney(precio);

            Debug.Log($"Compraste {engimono.Nombre} por {precio}. Dinero restante: {gameStats.GetDineroTotal()}");

            // Destruir el objeto comprado
            Destroy(gameObject);
        }
        else
        {
            Debug.Log($"No tienes suficiente dinero para comprar {engimono.Nombre}. Necesitas {precio}, tienes {dineroActual}.");
        }
    }
}