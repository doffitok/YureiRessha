using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "CharacterAlmaData", menuName = "YureiRessha/CharacterAlmaData", order = 0)]
public class CharacterAlmaData : ScriptableObject
{
    [Header("Identidad")]
    public string characterId;
    public string displayName;

    [Header("Texto")]
    [TextArea(3, 8)]
    public string description;

    [Header("Imágenes")]
    public Sprite portrait;                 // imagen grande (panel izquierdo)
    public Sprite[] gallerySprites = new Sprite[3]; // hasta 3 miniaturas abajo

    [Header("Botón del Almanaque")]
    public Sprite buttonBackground; // Fondo del botón
    public TMP_FontAsset buttonFont; // Fuente personalizada (opcional)
    public Color buttonTextColor = Color.white; // Color del texto
    public float buttonFontSize = 24f; // Tamaño de fuente TMP
}
