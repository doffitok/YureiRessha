using UnityEngine;

[CreateAssetMenu(fileName = "engimono", menuName = "Engimono/Engimono")]
public class ItemInventario : ScriptableObject
{
    // Datos generales
    public string ID;
    public string Nombre;
    public Sprite Icono;

    // Clase o categoria
    [Header("Clase del Engimono")]
    public ClaseEngimono clase;

    // La descripcion del objeto que le vamos a mostrar al jugador. Esto SI lo van a ver los jugadores asi que seamos bien descriptivos :D
    [Header("Descripción (dentro del juego)")]
    [TextArea(1, 5)] // Esto hace que el cuadro sea más grande en el Inspector
    public string descripcion;

    // Notas internas para nosotros nomas
    [Header("Notas (solo para nosotros)")]
    [TextArea(5, 10)]
    public string notas;

    // Aca asignamos el script correspondiente al objeto para que cada engimono tenga su propio efecto
    [Header("Efecto")]
    public MonoBehaviour efectoScript;

    // NUEVA SECCIÓN: Precios
    [Header("Precios")]
    public int Compra; // Precio al comprar
    public int Venta;  // Precio al vender

    // Metodo para ejecutar el efecto
    public void AplicarEfecto(GameObject objetivo)
    {
        if (efectoScript is engimonoEfecto efecto)
        {
            efecto.Aplicar(objetivo);
        }
        else
        {
            Debug.LogWarning($"El script asignado a {Nombre} no implementa engimonoEfecto");
        }
    }
}

public enum ClaseEngimono
{
    Economía,
    Suerte,
    Rating,
    Amistad
}

// Interfaz que todos los scripts de efecto deben implementar
public interface engimonoEfecto
{
    void Aplicar(GameObject objetivo);
}