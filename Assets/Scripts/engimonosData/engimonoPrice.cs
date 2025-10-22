using UnityEngine;
using TMPro;

public class engimonoPrice : MonoBehaviour
{
    [Header("Datos del Engimono")]
    public ItemInventario engimonoData; // ScriptableObject con los datos del engimono

    [Header("Referencias UI")]
    [SerializeField] private TextMeshProUGUI shopSlotPrice; // Texto donde se mostrará el precio

    private void Awake()
    {
        // Buscar automáticamente el texto si no está asignado
        if (shopSlotPrice == null)
        {
            Transform priceTransform = transform.Find("shopSlotPrice");
            if (priceTransform != null)
                shopSlotPrice = priceTransform.GetComponent<TextMeshProUGUI>();
        }
    }

    private void Start()
    {
        MostrarPrecio();
    }

    public void MostrarPrecio()
    {
        if (engimonoData != null && shopSlotPrice != null)
        {
            shopSlotPrice.text = "¥" + engimonoData.Compra.ToString("N0");
        }
        else
        {
            Debug.LogWarning($"[engimonoPrice] Faltan referencias en {name}: EngimonoData o TextMeshPro no asignados.");
        }
    }
}