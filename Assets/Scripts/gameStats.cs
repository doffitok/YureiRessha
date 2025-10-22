using UnityEngine;

// Este script centraliza las variables del juego: rating, dinero y suerte
// Ahora soporta modificadores aditivos para efectos externos
public class GameStats : MonoBehaviour
{
    [Header("Rating")]
    [Range(0, 100)]
    public int rating = 25;        // Valor base
    [HideInInspector] public int ratingExtra = 0; // Modificadores externos

    [Header("Dinero")]
    [Range(0, 10000)]
    public int dinero = 1000;      // Valor base
    [HideInInspector] public int dineroExtra = 0;  // Modificadores externos

    [Header("Suerte")]
    [Range(0, 100)]
    public int suerte = 1;         // Valor base
    [HideInInspector] public int suerteExtra = 0;  // Modificadores externos

    // --- Métodos para obtener valores totales ---
    public int GetRatingTotal()
    {
        return Mathf.Clamp(rating + ratingExtra, 0, 100); // Limitar a rango 0-100
    }

    public int GetDineroTotal()
    {
        return Mathf.Max(dinero + dineroExtra, 0); // No puede ser negativo
    }

    public int GetSuerteTotal()
    {
        return Mathf.Clamp(suerte + suerteExtra, 0, 100); // Limitar a rango 0-100
    }

    // --- Métodos de utilidad para aplicar modificadores ---
    public void AddRating(int value)
    {
        ratingExtra += value;
    }

    public void AddDinero(int value)
    {
        dineroExtra += value;
    }

    public void AddSuerte(int value)
    {
        suerteExtra += value;
    }

    // --- Método nuevo para gastar dinero directamente ---
    public void SpendMoney(int amount)
    {
        dinero = Mathf.Max(dinero - amount, 0);
    }
}