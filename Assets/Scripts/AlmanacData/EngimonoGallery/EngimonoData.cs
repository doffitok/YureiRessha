using UnityEngine;

[CreateAssetMenu(fileName = "EngimonoData", menuName = "YureiRessha/EngimonoData")]
public class EngimonoData : ScriptableObject
{
    [Header("Identidad")]
    public string engimonoName;

    [Header("Descripción")]
    [TextArea(2, 4)]
    public string engimonoDescription;

    [Header("Sprites")]
    public Sprite engimonoIcon;           // Icono pequeño (para el ScrollView)
    public Sprite engimonoMainSprite;     // Imagen principal (para el Info Panel)

    [Header("Fondo o color opcional")]
    public Sprite backgroundSprite;       // Fondo de la tarjeta
    public Color backgroundColor = Color.white;

    [Header("Opciones de Hover")]
    public bool highlightOnHover = true;
    public Vector2 panelOffset = new Vector2(400, 0);
}
