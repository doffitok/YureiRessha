using UnityEngine;

////////////////////////////////////////////////////////////////////////////////////////////
// este script centraliza las variables globales del juego (rating, dinero y suerte)
// cada estadistica tiene un valor base y un valor extra que se usa para modificadores de items, efectos o buffs temporales 
// las funciones GetXXXTotal devuelven el valor combinado y clamped cuando corresponde
// los extras permiten que otros sistemas sumen efectos sin tocar los valores base
////////////////////////////////////////////////////////////////////////////////////////////

[DisallowMultipleComponent]
public class GameStats : MonoBehaviour
{
    ////////////////////////////////////////////////////////////////////////////////////////////
    // rating general del jugador
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Rating")]
    [Range(0, 100)]
    public int rating = 25;                // valor base editable
    [HideInInspector] public int ratingExtra = 0; // modificadores externos (items, bonus, etc)

    ////////////////////////////////////////////////////////////////////////////////////////////
    // dinero actual del jugador
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Dinero")]
    public int dinero = 1000;              // valor base inicial
    [HideInInspector] public int dineroExtra = 0; // modificadores externos (ganancias, pasivos, etc)

    ////////////////////////////////////////////////////////////////////////////////////////////
    // suerte general del jugador
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Suerte")]
    [Range(0, 100)]
    public int suerte = 1;                 // valor base inicial
    [HideInInspector] public int suerteExtra = 0; // modificadores externos (items, boosts, etc)

    ////////////////////////////////////////////////////////////////////////////////////////////
    // metodos para obtener los valores combinados
    ////////////////////////////////////////////////////////////////////////////////////////////
    public int GetRatingTotal()
    {
        return Mathf.Clamp(rating + ratingExtra, 0, 100); // el rating no puede superar 100
    }

    public int GetDineroTotal()
    {
        return dinero + dineroExtra; // el dinero puede ser negativo
    }

    public int GetSuerteTotal()
    {
        return Mathf.Clamp(suerte + suerteExtra, 0, 100); // la suerte tampoco supera 100
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // metodos para modificar valores extra
    ////////////////////////////////////////////////////////////////////////////////////////////
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

    ////////////////////////////////////////////////////////////////////////////////////////////
    // metodo directo para gastar dinero
    ////////////////////////////////////////////////////////////////////////////////////////////
    public void SpendMoney(int amount)
    {
        dinero -= amount; // puede volverse negativo para irse a una deuda (no recuerdo iesto esta realmente implementado pero bueno)
    }
}