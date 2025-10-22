using UnityEngine;

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
}