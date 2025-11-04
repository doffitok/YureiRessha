using UnityEngine;

[DisallowMultipleComponent]
public class EngimonosInventoryManager : MonoBehaviour
{
    [Header("Contenedor principal de slots (Grid/VerticalLayout)")]
    [SerializeField] private Transform slotsParent;

    private void Awake()
    {
        if (slotsParent == null)
        {
            // Si no está asignado manualmente, usa el primer hijo visible
            if (transform.childCount > 0)
                slotsParent = transform.GetChild(0);
            else
                slotsParent = transform;
        }
    }

    /// <summary>
    /// Devuelve el primer slot sin hijos, o el contenedor principal si no hay espacio.
    /// </summary>
    public Transform ObtenerSlotDisponible()
    {
        if (slotsParent == null)
        {
            Debug.LogWarning("[Inventory] ⚠ slotsParent no asignado, usando self.");
            return transform;
        }

        foreach (Transform slot in slotsParent)
        {
            if (slot.childCount == 0)
                return slot;
        }

        Debug.LogWarning("[Inventory] ⚠ No hay slots libres, devolviendo el contenedor principal.");
        return slotsParent;
    }

    /// <summary>
    /// Agrega un Engimono al inventario visual y aplica su efecto automáticamente.
    /// </summary>
    public void AgregarAlInventario(EngimonoItem item)
    {
        if (item == null)
        {
            Debug.LogWarning("[Inventory] ❌ Item nulo al intentar agregar al inventario.");
            return;
        }

        Transform destino = ObtenerSlotDisponible();
        if (destino == null)
        {
            Debug.LogWarning("[Inventory] ❌ No hay slot válido para colocar el item.");
            return;
        }

        // Crear la copia visual del Engimono dentro del inventario
        var copia = item.DuplicarEnInventario(destino);
        copia.Comprado = true;
        copia.ActualizarUI();

        // Aplica el efecto automáticamente una vez
        var efecto = copia.GetComponent<IEngimonoApply>();
        if (efecto != null)
        {
            efecto.AplicarEfecto(FindFirstObjectByType<GameStats>().gameObject);
            Debug.Log($"[{copia.Nombre}] ✨ Efecto aplicado automáticamente al agregarse al inventario.");
        }
        else
        {
            Debug.LogWarning($"[{copia.Nombre}] ⚠ No se encontró componente IEngimonoApply al agregarse al inventario.");
        }

        Debug.Log($"[Inventory] ✅ '{copia.Nombre}' agregado correctamente al slot '{destino.name}'.");
    }
}