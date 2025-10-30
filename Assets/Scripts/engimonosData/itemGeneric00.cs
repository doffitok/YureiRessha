using UnityEngine;

/// <summary>
/// Item genérico #01: suma +10 al Rating total del jugador.
/// </summary>
public class ItemGeneric01 : MonoBehaviour, IEngimonoApply
{
    [Tooltip("Cantidad de Rating adicional que otorga este ítem.")]
    public int cantidad = 10;

    public void AplicarEfecto(GameObject objetivo)
    {
        GameStats stats = objetivo.GetComponent<GameStats>();
        if (stats == null)
        {
            Debug.LogWarning("[ItemGeneric01] No se encontró GameStats en el objetivo.");
            return;
        }

        stats.ratingExtra += cantidad;
        Debug.Log($"[ItemGeneric01] +{cantidad} al Rating aplicado correctamente.");
    }
}