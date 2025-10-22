using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class uiEngimonoShopInfo : MonoBehaviour
{
    [Header("ScriptableObject del Engimono")]
    public ItemInventario engimonoData;

    private Image iconImage;

    void Awake()
    {
        // Tomar automáticamente el Image del mismo GameObject
        iconImage = GetComponent<Image>();
    }

    void Start()
    {
        ActualizarIcono();
    }

    /// <summary>
    /// Asigna el icono del ScriptableObject al Image del GameObject.
    /// </summary>
    public void ActualizarIcono()
    {
        if (engimonoData == null)
        {
            Debug.LogWarning("No se ha asignado ningún ItemInventario.");
            iconImage.enabled = false;
            return;
        }

        if (engimonoData.Icono != null)
        {
            iconImage.sprite = engimonoData.Icono;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
            Debug.LogWarning($"El Engimono {engimonoData.Nombre} no tiene un icono asignado.");
        }
    }
}