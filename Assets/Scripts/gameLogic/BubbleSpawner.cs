using UnityEngine;
using System.Collections;
using TMPro;

public class BubbleSpawner : MonoBehaviour
{
    [Header("Configuración de Burbujas")]
    public GameObject bubblePrefab;
    public Transform bubbleSpawnPoint;
    public float bubbleDuration = 3f;
    
    [Header("Referencias")]
    public CharacterData characterData;
    
    private GameObject currentBubble;
    private bool canSpawnBubble = true;

    void Start()
    {
        // Si no tenemos spawn point, usar este objeto
        if (bubbleSpawnPoint == null)
            bubbleSpawnPoint = this.transform;
        
        StartCoroutine(BubbleSpawningRoutine());
    }

    IEnumerator BubbleSpawningRoutine()
    {
        while (true)
        {
            if (canSpawnBubble && ShouldSpawnBubble())
            {
                SpawnBubble();
                
                // Tiempo de espera basado en demanda
                float waitTime = GetWaitTimeBetweenBubbles();
                yield return new WaitForSeconds(waitTime);
            }
            yield return new WaitForSeconds(1f); // Revisar cada segundo
        }
    }

    public bool ShouldSpawnBubble()
    {
        if (characterData == null) return false;

        // Usar DEMANDA en lugar de exigencia
        float demandaNormalizada = (characterData.demandaMin + characterData.demandaMax) / 200f;
        float spawnChance = Mathf.Clamp(demandaNormalizada, 0.1f, 0.8f);

        return Random.Range(0f, 1f) < spawnChance;
    }

    private float GetWaitTimeBetweenBubbles()
    {
        if (characterData == null) return 10f;

        // Mayor demanda = menos espera entre burbujas
        float demandaPromedio = (characterData.demandaMin + characterData.demandaMax) / 2f;
        return Mathf.Lerp(15f, 3f, demandaPromedio / 100f);
    }

    public void SpawnBubble()
    {
        if (bubblePrefab == null) 
        {
            Debug.LogWarning("No hay bubblePrefab asignado en " + gameObject.name);
            return;
        }

        // Destruir burbuja anterior si existe
        if (currentBubble != null)
            Destroy(currentBubble);

        // Crear nueva burbuja
        Vector3 spawnPosition = bubbleSpawnPoint.position + Vector3.up * 1.5f;
        currentBubble = Instantiate(bubblePrefab, spawnPosition, Quaternion.identity);
        
        // Configurar la burbuja
        SetupBubbleContent(currentBubble);
        
        // Destruir después de un tiempo
        StartCoroutine(DestroyBubbleAfterTime(currentBubble, bubbleDuration));
    }

    private IEnumerator DestroyBubbleAfterTime(GameObject bubble, float duration)
    {
        yield return new WaitForSeconds(duration);
        
        if (bubble != null && bubble == currentBubble)
        {
            Destroy(bubble);
        }
    }

    private void SetupBubbleContent(GameObject bubble)
    {
        // Buscar componentes en la burbuja
        SpriteRenderer spriteRenderer = bubble.GetComponent<SpriteRenderer>();
        TMPro.TextMeshPro textMesh = bubble.GetComponentInChildren<TMPro.TextMeshPro>();

        if (characterData != null)
        {
            // Configurar color basado en exigencia
            if (spriteRenderer != null)
            {
                spriteRenderer.color = GetBubbleColorByExigence();
            }

            // Configurar texto
            if (textMesh != null)
            {
                textMesh.text = GetBubbleText();
            }
        }
    }

    private Color GetBubbleColorByExigence()
    {
        if (characterData == null) return Color.white;

        if (characterData.exigencia <= 5)
            return new Color(0.4f, 0.8f, 0.4f);   // Verde claro
        else if (characterData.exigencia <= 10)
            return new Color(0.9f, 0.8f, 0.3f);   // Amarillo
        else if (characterData.exigencia <= 15)
            return new Color(0.9f, 0.6f, 0.2f);   // Naranja
        else
            return new Color(0.8f, 0.3f, 0.3f);   // Rojo
    }

    private string GetBubbleText()
    {
        if (characterData == null) return "?";

        // Texto basado en exigencia
        if (characterData.exigencia <= 3)
            return "😊\nRelajado";
        else if (characterData.exigencia <= 7)
            return "😐\nNormal";
        else if (characterData.exigencia <= 12)
            return "😠\nExigente";
        else if (characterData.exigencia <= 16)
            return "😤\nMuy exigente";
        else
            return "🤬\nExtremo!";
    }

    // Método público para forzar spawn desde otros scripts
    public void ForceSpawnBubble()
    {
        SpawnBubble();
    }

    // Método para spawn con texto personalizado
    public void SpawnCustomBubble(string customText)
    {
        SpawnBubble();
        
        if (currentBubble != null)
        {
            TMPro.TextMeshPro textMesh = currentBubble.GetComponentInChildren<TMPro.TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = customText;
            }
        }
    }
}