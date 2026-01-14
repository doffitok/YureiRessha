using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;

[DisallowMultipleComponent]
public class EngimonoItem : MonoBehaviour, IPointerClickHandler
{
    [Header("Datos del Engimono")]
    public string ID = "default_id";
    public string Nombre = "Nuevo Engimono";
    [TextArea(2, 5)] public string Descripcion = "Descripcion generica del Engimono.";
    public Sprite Icono;
    public int PrecioCompra = 500;
    public int PrecioVenta = 250;
    public bool Comprado = false;

    [Header("Referencias UI")]
    [SerializeField] private Image iconoUI;
    [SerializeField] private TextMeshProUGUI nombreTexto;
    [SerializeField] private TextMeshProUGUI precioTexto; // Se vincula a shopSlotPricetag/shopSlotPrice

    [Header("Sistemas externos")]
    [SerializeField] private GameStats gameStats;
    [SerializeField] private EngimonosInventoryManager inventory;

    private void Awake()
    {
        if (gameStats == null) gameStats = FindFirstObjectByType<GameStats>();
        if (inventory == null) inventory = FindFirstObjectByType<EngimonosInventoryManager>();
        if (iconoUI == null) iconoUI = ResolverImageDeIcono();

        // 🔹 Vincular automáticamente el texto del precio si no se asignó manualmente
        if (precioTexto == null)
        {
            var priceObj = transform.Find("shopSlotPricetag/shopSlotPrice");
            if (priceObj != null)
                precioTexto = priceObj.GetComponent<TextMeshProUGUI>();
        }

        ActualizarUI();
    }

    private void OnEnable() => ActualizarUI();

    // Clic en el objeto
    public void OnPointerClick(PointerEventData eventData)
    {
        if (Comprado)
        {
            Debug.Log($"[{Nombre}] 🔒 Ya comprado — no puede volver a activarse ni comprarse.");
            return;
        }

        Debug.Log($"[{Nombre}] 🛒 Clic detectado — intentando comprar...");
        IntentarCompra();
    }

    // Lógica de compra
    private void IntentarCompra()
    {
        if (gameStats == null || inventory == null)
        {
            Debug.LogWarning($"[{Nombre}] ❌ No se puede comprar: faltan referencias externas.");
            return;
        }

        if (gameStats.GetDineroTotal() < PrecioCompra)
        {
            Debug.Log($"[{Nombre}] 💸 Dinero insuficiente ({gameStats.GetDineroTotal()}/{PrecioCompra}).");
            return;
        }

        // 1️⃣ Restar dinero
        gameStats.SpendMoney(PrecioCompra);

        // 2️⃣ Marcar como comprado
        Comprado = true;

        // 3️⃣ Agregar al inventario
        inventory.AgregarAlInventario(this);

        // 4️⃣ Actualizar visual
        ActualizarUI();

        Debug.Log($"[{Nombre}] 🛍️ Has comprado este engimono correctamente por ¥{PrecioCompra:N0}.");

        // 5️⃣ Eliminar el original de la tienda
        Destroy(gameObject);
    }

    // Mostrar visualmente el icono y nombre
    public void ActualizarUI()
    {
        if (iconoUI != null && Icono != null)
        {
            iconoUI.sprite = Icono;
            iconoUI.enabled = true;
        }

        if (nombreTexto != null)
            nombreTexto.text = Nombre;

        if (precioTexto != null)
        {
            if (Comprado)
            {
                precioTexto.text = "";
            }
            else
            {
                precioTexto.text = $"¥{PrecioCompra:N0}";
            }
        }
    }

    // Crea la copia visual en el inventario
    public EngimonoItem DuplicarEnInventario(Transform parent)
    {
        GameObject copiaGO = Instantiate(gameObject, parent);
        EngimonoItem copia = copiaGO.GetComponent<EngimonoItem>();

        copia.ID = ID;
        copia.Nombre = Nombre;
        copia.Descripcion = Descripcion;
        copia.Icono = Icono;
        copia.PrecioCompra = PrecioCompra;
        copia.PrecioVenta = PrecioVenta;
        copia.Comprado = true;

        copia.ActualizarUI();

        RectTransform rect = copiaGO.transform as RectTransform;
        if (rect != null)
        {
            rect.localScale = Vector3.one;
            rect.anchoredPosition = Vector2.zero;
            rect.localRotation = Quaternion.identity;
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        else
        {
            copiaGO.transform.localScale = Vector3.one;
            copiaGO.transform.localPosition = Vector3.zero;
            copiaGO.transform.localRotation = Quaternion.identity;
        }

        return copia;
    }

    private Image ResolverImageDeIcono()
    {
        var imgs = GetComponentsInChildren<Image>(true);
        var target = imgs.FirstOrDefault(i => NombrePareceIcono(i.name));
        if (target == null && imgs.Length > 0)
            target = imgs[0];
        return target;
    }

    private bool NombrePareceIcono(string n)
    {
        if (string.IsNullOrEmpty(n)) return false;
        n = n.ToLowerInvariant();
        return n.Contains("icon") || n.Contains("icono") || n.Contains("thumb");
    }
}