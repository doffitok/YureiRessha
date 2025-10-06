using UnityEngine;

[CreateAssetMenu(fileName = "characters", menuName = "Characters/CharacterData")]
public class CharacterData : ScriptableObject
{
    [Header("Datos")]
    public string ID;
    public string nombre;

    [Header("Variables")]
    [Range(0, 20)] public int rating;      
    [Range(0, 20)] public int dinero;      

    [Header("Demanda")]
    [Range(0, 100)] public int demandaMin = 1;   // valor minimo para el roll
    [Range(0, 100)] public int demandaMax = 100;  // valor maximo para el roll

    [Range(0, 20)] public int exigencia;   

    [Header("Debug")]
    public Color debugColor;               
    public Texture2D debugTexture;         

    [Header("Notas")]
    [TextArea(10, 10)]
    public string notas;
}