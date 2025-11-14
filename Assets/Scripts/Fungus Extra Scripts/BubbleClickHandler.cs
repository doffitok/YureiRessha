using UnityEngine;
using Fungus; // ← AÑADE ESTA LÍNEA

public class BubbleClickHandler : MonoBehaviour
{
    private BubbleSpawner bubbleSpawner;
    private CharacterData characterData;

    void Start()
    {
        // Buscar el BubbleSpawner en el padre (el personaje)
        bubbleSpawner = GetComponentInParent<BubbleSpawner>();
        if (bubbleSpawner != null)
        {
            characterData = bubbleSpawner.characterData;
        }
    }

    void OnMouseDown()
    {
        if (characterData != null)
        {
            // Cambiar amistad aleatoriamente
            int cambio = Random.Range(-2, 3); // -2, -1, 0, 1, 2
            characterData.CambiarAmistad(cambio);
            
            Debug.Log($"Burbuja clickeada! {characterData.nombre}: Amistad {cambio} = {characterData.amistad} ({characterData.EstadoActual})");
            
            // Iniciar conversación en Fungus basada en el estado emocional
            IniciarConversacionFungus();
            
            // Destruir la burbuja después del click
            Destroy(gameObject);
        }
    }

    void IniciarConversacionFungus()
    {
        // Buscar Flowchart en la escena
        Flowchart flowchart = FindObjectOfType<Flowchart>();
        if (flowchart != null && characterData != null)
        {
            string bloqueConversacion = "Conversacion" + characterData.EstadoActual.ToString();
            
            // Ejecutar el bloque correspondiente al estado emocional
            if (flowchart.HasBlock(bloqueConversacion))
            {
                flowchart.ExecuteBlock(bloqueConversacion);
            }
            else
            {
                Debug.LogWarning($"No se encontró el bloque Fungus: {bloqueConversacion}");
            }
        }
    }
}