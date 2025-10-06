using UnityEngine;

[CreateAssetMenu(fileName = "characters", menuName = "Characters/CharacterData")]
public class CharacterData : ScriptableObject
{
    [Header("Datos")]
    public string ID;
    public string nombre;

    [Header("Variables")]
    [Range(0, 20)] public int rating;      // Slider del 0 al 20
    [Range(0, 20)] public int dinero;      // Slider del 0 al 20
    [Range(0, 20)] public int demanda;     // Slider del 0 al 20
    [Range(0, 20)] public int exigencia;   // Slider del 0 al 20

    [Header("Debug")]
    public Color debugColor;               // Color que se le asignara a objetos de prueba para identificar personajes
    public Texture2D debugTexture;         // Textura que se usara para depuracion o identificacion visual

    // Aca podemos escribir notas del personaje y cualquier cosa que estimemos pertinentes :D
    [Header("Notas")]
    [TextArea(10, 10)]
    public string notas;
}
