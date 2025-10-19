using UnityEngine;

[CreateAssetMenu(fileName = "NuevoAlmanaqueInfo", menuName = "Almanaque/Almanaque Info")]
public class AlmanaqueInfo : ScriptableObject
{
    [Header("Información del Personaje")]
    public string titulo;

    [TextArea(3, 10)]
    public string descripcion;

    [Header("Imágenes")]
    public Sprite spritePersonaje;  
    public Sprite spriteExtra;       
}
