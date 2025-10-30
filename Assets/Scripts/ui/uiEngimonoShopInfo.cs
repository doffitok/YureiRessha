using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class uiEngimonoShopInfo : MonoBehaviour
{
    [Header("Datos")]
    public EngimonosData engimonoData;  

    [Header("Decoraciones de TIENDA (se destruyen al comprar)")]
    [Tooltip("Rutas relativas de objetos a destruir al comprar (ej: 'shopSlotPriceTag' o 'shopSlotPriceTag/shopSlotPrice')")]
    public string[] shopDecorPaths = new string[] { "shopSlotPriceTag", "shopSlotPriceTag/shopSlotPrice" };

    private Image iconImage;

    private void Awake()
    {
        iconImage = GetComponent<Image>();
    }

    private void Start()
    {
        ActualizarIcono();
    }

    public void ActualizarIcono()
    {
        if (engimonoData == null)
        {
            iconImage.enabled = false;
            Debug.LogWarning("[uiEngimonoShopInfo] Sin engimonoData.");
            return;
        }

        iconImage.enabled = true;
        iconImage.sprite = engimonoData.Icono;
    }

    /// <summary>
    /// Elimina decoraciones de la tarjeta de TIENDA (precio/etiquetas).
    /// </summary>
    public void BorrarDecorTienda()
    {
        foreach (var path in shopDecorPaths)
        {
            if (string.IsNullOrEmpty(path)) continue;
            var t = transform.Find(path);
            if (t != null) Destroy(t.gameObject);
        }
    }
}