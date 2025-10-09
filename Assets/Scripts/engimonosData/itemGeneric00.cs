using UnityEngine;

// Este script suma +10 al Rating total del GameStats una sola vez
public class SumaRating : MonoBehaviour
{
    private GameStats stats;

    private void Awake()
    {
        stats = FindFirstObjectByType<GameStats>();
        if (stats == null)
        {
            Debug.LogWarning("[SumaRating] No se encontró GameStats en la escena.");
            return;
        }

        stats.ratingExtra = 10;
    }
}