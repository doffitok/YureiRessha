using UnityEngine;

// Este script centraliza las variables del juego: rating, dinero y suerte
// Puedes cambiar sus rangos maximos, minimos y valores por defecto modificando
// los comentarios indicados debajo de cada variable

public class GameStats : MonoBehaviour
{
    // rating: calificacion del tren
    [Header("Rating")]
    [Range(1,5)] // Rango minimo y maximo
    public int rating = 3; // Valor por defecto

    // dinero: dinero disponible del tren
    [Header("Dinero")]
    [Range(0,10000)] // Rango minimo y maximo
    public int dinero = 1000; // Valor por defecto

    // suerte: factor aleatorio que puede alterar probabilidades
    [Header("Suerte")]
    [Range(0,10)] // Rango minimo y maximo
    public int suerte = 1; // Valor por defecto
}