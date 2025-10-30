using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class EngimonoShopItem : MonoBehaviour, IPointerClickHandler
{
    [Header("Datos del Engimono")]
    public EngimonosData engimonoData;

    [Header("Estado")]
    [Tooltip("Indica si este objeto ya fue comprado. Bloquea la compra si es true.")]
    public bool Comprado = false;

    [Header("Referencias UI (pueden autocompletarse)")]
    [SerializeField] private Image icono;
    [SerializeField] private TextMeshProUGUI nombreTexto;
    [SerializeField] private TextMeshProUGUI precioTexto;

    [Header("Sistemas externos")]
    [SerializeField] private GameStats gameStats;
    [SerializeField] private uiEngimonosInventoryManager inventory;

    private IEngimonoApply efecto;

    private void Awake()
    {
        // Autodetección de UI
        if (icono == null) icono = transform.GetComponentInChildren<Image>(true);
        if (nombreTexto == null) nombreTexto = BuscarTMP(new string[] { "nombre", "name", "title" });
        if (precioTexto == null) precioTexto = BuscarTMP(new string[] { "precio", "price", "shopslotprice" });

        // Sistemas
        if (gameStats == null) gameStats = FindFirstObjectByType<GameStats>();
        if (inventory == null) inventory = uiEngimonosInventoryManager.Instance;

        efecto = GetComponent<IEngimonoApply>();
    }

    private void OnEnable()
    {
        // Si este ítem NO está en la jerarquía de la tienda, considéralo ya comprado (es inventario)
        if (!EstaEnTienda()) Comprado = true;

        // Si no es de tienda, ocultar decoraciones de precio (si existen)
        if (Comprado) OcultarDecoracionTienda();

        ActualizarUI();
    }

    private TextMeshProUGUI BuscarTMP(string[] posiblesNombres)
    {
        foreach (var t in GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            string lower = t.name.ToLower();
            foreach (string palabra in posiblesNombres)
                if (lower.Contains(palabra.ToLower())) return t;
        }
        return null;
    }

    public void ActualizarUI()
    {
        if (engimonoData == null) return;

        if (icono != null) { icono.sprite = engimonoData.Icono; icono.enabled = true; }
        if (nombreTexto != null) nombreTexto.text = engimonoData.Nombre;

        // El precio solo tiene sentido en la tienda (no en inventario)
        if (precioTexto != null)
        {
            if (!Comprado && EstaEnTienda())
                precioTexto.text = "¥" + engimonoData.Compra.ToString("N0");
            else
                precioTexto.text = ""; // limpia en inventario o si ya se compró
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Bloquear click si ya fue comprado (inventario) o si no está en tienda
        if (Comprado || !EstaEnTienda()) return;

        IntentarCompra();
    }

    private void IntentarCompra()
    {
        if (engimonoData == null || gameStats == null || inventory == null) return;

        int precio = engimonoData.Compra;
        if (gameStats.GetDineroTotal() < precio) { Debug.Log("Dinero insuficiente."); return; }

        // 1) restar dinero
        gameStats.SpendMoney(precio);

        // 2) agregar al inventario (el prefab que uses allí debe tener Comprado=true al instanciar, o lo forzamos abajo)
        inventory.AddEngimono(new EngimonoInstance { data = engimonoData, comprado = true });

        // 3) aplicar efecto
        efecto?.AplicarEfecto(gameStats.gameObject);

        // 4) marcar como comprado y eliminar el objeto de TIENDA
        Comprado = true;
        Destroy(gameObject); // << vuelve el comportamiento anterior (desaparece de la tienda)
    }

    private bool EstaEnTienda()
    {
        // Heurística simple: si algún padre contiene "shop" en el nombre, lo consideramos de tienda
        Transform t = transform;
        while (t != null)
        {
            string n = t.name.ToLower();
            if (n.Contains("shop")) return true;
            t = t.parent;
        }
        return false;
    }

    private void OcultarDecoracionTienda()
    {
        // Si usas la etiqueta visual de precio en la tienda, elimínala en inventario
        var tag = transform.Find("shopSlotPriceTag");
        if (tag != null) tag.gameObject.SetActive(false);
    }
}