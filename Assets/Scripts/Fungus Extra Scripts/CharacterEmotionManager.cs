using UnityEngine;
using Fungus;

public class CharacterEmotionManager : MonoBehaviour
{
    [Header("Configuración")]
    public CharacterData characterData;
    public Flowchart flowchart;

    [Header("Variables Fungus")]
    public string variableAmistad = "AmistadActual";
    public string variableEstado = "EstadoEmocional";
    public string variableCharacterID = "CharacterID";

    void Start()
    {
        ActualizarVariablesFungus();
    }

    public void ActualizarVariablesFungus()
    {
        if (flowchart != null && characterData != null)
        {
            // Actualizar variables en Fungus
            flowchart.SetIntegerVariable(variableAmistad, characterData.amistad);
            flowchart.SetStringVariable(variableEstado, characterData.EstadoActual.ToString());
            flowchart.SetStringVariable(variableCharacterID, characterData.ID);
        }
    }

    // Llamar este método cuando cambie la amistad
    public void OnAmistadChanged()
    {
        ActualizarVariablesFungus();
    }
}