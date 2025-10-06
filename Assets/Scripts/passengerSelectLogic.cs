using UnityEngine;

public class passengerSelectLogic : MonoBehaviour
{
    [Header("Referencias")]
    public passengerPlacementLogic placementLogic; // Referencia al script que tiene los placeholders

    private CharacterData[] allCharacters;

    void Start()
    {
        if (placementLogic == null)
        {
            Debug.LogError("[passengerSelectLogic] No se ha asignado passengerPlacementLogic en el inspector.");
            return;
        }

        // Cargar todos los CharacterData dentro de Resources/charactersData
        allCharacters = Resources.LoadAll<CharacterData>("charactersData");

        if (allCharacters.Length == 0)
        {
            Debug.LogError("[passengerSelectLogic] ¡Error! No se encontraron personajes en 'Resources/charactersData'");
            return;
        }

        // Mostrar los nombres de los personajes encontrados
        Debug.Log("[passengerSelectLogic] Personajes encontrados:");
        foreach (CharacterData character in allCharacters)
        {
            Debug.Log($" - {character.nombre}");
        }

        // Obtener todos los placeholders desde passengerPlacementLogic
        Transform[] placeholders = placementLogic.GetPassengerPlaceholders();

        foreach (Transform placeholder in placeholders)
        {
            Debug.Log($"[passengerSelectLogic] Analizando placeholder: {placeholder.name}");

            // Buscar cualquier SpriteRenderer hijo del placeholder
            SpriteRenderer spriteRenderer = placeholder.GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer == null)
            {
                Debug.LogWarning($"[passengerSelectLogic] No se encontró SpriteRenderer en {placeholder.name}");
                continue;
            }

            // Seleccionar un personaje aleatorio
            CharacterData randomCharacter = allCharacters[Random.Range(0, allCharacters.Length)];

            if (randomCharacter.debugTexture == null)
            {
                Debug.LogWarning($"[passengerSelectLogic] El personaje {randomCharacter.nombre} no tiene debugTexture asignada.");
                continue;
            }

            // Forzar refresco del sprite
            spriteRenderer.sprite = null;

            // Crear un nuevo sprite a partir de la textura del CharacterData
            Sprite newSprite = Sprite.Create(
                randomCharacter.debugTexture,
                new Rect(0, 0, randomCharacter.debugTexture.width, randomCharacter.debugTexture.height),
                new Vector2(0.5f, 0.5f)
            );

            spriteRenderer.sprite = newSprite;
            Debug.Log($"[passengerSelectLogic] Asignado sprite de {randomCharacter.nombre} a {placeholder.name}");
        }
    }
}