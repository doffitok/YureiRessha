using UnityEngine;
using UnityEngine.UI;

public class DebugAmistad : MonoBehaviour
{
    [Header("Referencias")]
    public CharacterData characterData;
    public Button botonSubirAmistad;
    public Button botonBajarAmistad;
    public Text textoEstado;

    void Start()
    {
        // Configurar botones
        botonSubirAmistad.onClick.AddListener(SubirAmistad);
        botonBajarAmistad.onClick.AddListener(BajarAmistad);
        
        ActualizarUI();
    }

    public void SubirAmistad()
    {
        characterData.CambiarAmistad(1);
        ActualizarUI();
        Debug.Log($"{characterData.nombre}: Amistad +1 = {characterData.amistad} ({characterData.EstadoActual})");
    }

    public void BajarAmistad()
    {
        characterData.CambiarAmistad(-1);
        ActualizarUI();
        Debug.Log($"{characterData.nombre}: Amistad -1 = {characterData.amistad} ({characterData.EstadoActual})");
    }

    void ActualizarUI()
    {
        if (textoEstado != null)
        {
            textoEstado.text = $"{characterData.nombre}\nAmistad: {characterData.amistad}\nEstado: {characterData.EstadoActual}";
        }
    }
}