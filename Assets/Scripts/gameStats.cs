using UnityEngine;

// Este script centraliza las variables del juego: rating, dinero y suerte
// Soporta modificadores aditivos para efectos externos
public class GameStats : MonoBehaviour
{
    [Header("Rating")]
    [Range(0, 100)]
    public int rating = 25;        // Valor base
    [HideInInspector] public int ratingExtra = 0; // Modificadores externos

    [Header("Dinero")]
    public int dinero = 1000;      // Valor base editable sin límite
    [HideInInspector] public int dineroExtra = 0;  // Modificadores externos

    [Header("Suerte")]
    [Range(0, 100)]
    public int suerte = 1;         // Valor base
    [HideInInspector] public int suerteExtra = 0;  // Modificadores externos

    // --- Métodos para obtener valores totales ---
    public int GetRatingTotal()
    {
        return Mathf.Clamp(rating + ratingExtra, 0, 100); // rating sigue limitado
    }

    public int GetDineroTotal()
    {
        return dinero + dineroExtra; // ahora puede ser negativo
    }

    public int GetSuerteTotal()
    {
        return Mathf.Clamp(suerte + suerteExtra, 0, 100); // suerte sigue limitado
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

    // --- Método para gastar dinero directamente ---
    public void SpendMoney(int amount)
    {
        dinero -= amount; // puede volverse negativo
    }
}