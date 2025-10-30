using UnityEngine;

/// <summary>
/// Interface para cualquier script que aplique el efecto de un Engimono.
/// </summary>
public interface IEngimonoApply
{
    /// <summary>
    /// Ejecuta el efecto del Engimono sobre el objetivo indicado (por ejemplo, GameStats).
    /// </summary>
    /// <param name="objetivo">El GameObject afectado por el efecto.</param>
    void AplicarEfecto(GameObject objetivo);
}