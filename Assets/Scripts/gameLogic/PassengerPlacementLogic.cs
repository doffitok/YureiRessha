using System.Collections.Generic;
using UnityEngine;

public class PassengerPlacementLogic : MonoBehaviour
{
    [Header("Prefabs de pasajeros")]
    public List<GameObject> passengerPrefabs = new List<GameObject>();

    [Header("Personajes default (usados si no hay prefabs disponibles)")]
    public List<GameObject> defaultPassengers = new List<GameObject>();

    [Header("Posiciones donde se colocaran los pasajeros (detectadas automaticamente)")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("Rango de tirada para el rating")]
    [Range(0, 100)] public int minRatingRoll = 0;
    [Range(0, 100)] public int maxRatingRoll = 100;

    private GameStats gameStats;

    private void Awake()
    {
        gameStats = FindFirstObjectByType<GameStats>();
        if (gameStats == null)
            Debug.LogError("[PassengerPlacementLogic] No se encontro GameStats en la escena.");

        AutoDetectSpawnPoints();
    }

    private void AutoDetectSpawnPoints()
    {
        GameObject parent = GameObject.Find("passengerSpawns");
        if (parent != null)
        {
            spawnPoints.Clear();
            foreach (Transform child in parent.transform)
                spawnPoints.Add(child);

            Debug.Log("[PassengerPlacementLogic] Detectados " + spawnPoints.Count + " spawn points en 'passengerSpawns'.");
        }
        else
        {
            Debug.LogError("[PassengerPlacementLogic] No se encontro el objeto 'passengerSpawns' en la escena.");
        }
    }

    public List<GameObject> SpawnPassengers(List<GameObject> customPassengerPrefabs = null)
    {
        List<GameObject> prefabsToUse = customPassengerPrefabs ?? passengerPrefabs;

        if (prefabsToUse == null || prefabsToUse.Count == 0)
        {
            Debug.LogWarning("[PassengerPlacementLogic] No se encontraron pasajeros, se usaran personajes default.");
            prefabsToUse = defaultPassengers;
        }

        if (prefabsToUse == null || prefabsToUse.Count == 0)
        {
            Debug.LogError("[PassengerPlacementLogic] No hay personajes default disponibles. No se pueden instanciar pasajeros.");
            return new List<GameObject>();
        }

        // Limpiar pasajeros existentes
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        int spawnCount = spawnPoints.Count;

        // minimo garantizado
        int passengersToSpawn = 2;

        // se agregan pasajeros extra dependiendo del rating del jugador
        if (gameStats != null)
        {
            int extraSeats = spawnCount - 2;
            for (int i = 0; i < extraSeats; i++)
            {
                int roll = Random.Range(minRatingRoll, maxRatingRoll + 1);
                if (roll <= gameStats.rating)
                    passengersToSpawn++;
            }
        }

        passengersToSpawn = Mathf.Min(passengersToSpawn, prefabsToUse.Count);
        passengersToSpawn = Mathf.Min(passengersToSpawn, spawnCount);

        List<GameObject> availablePrefabs = new List<GameObject>(prefabsToUse);
        List<Transform> availableSpawns = new List<Transform>(spawnPoints);

        List<GameObject> instantiatedPassengers = new List<GameObject>();

        for (int i = 0; i < passengersToSpawn; i++)
        {
            if (availablePrefabs.Count == 0 || availableSpawns.Count == 0)
                break;

            int indexPrefab = Random.Range(0, availablePrefabs.Count);
            GameObject prefab = availablePrefabs[indexPrefab];
            availablePrefabs.RemoveAt(indexPrefab);

            int indexSpawn = Random.Range(0, availableSpawns.Count);
            Transform spawnPoint = availableSpawns[indexSpawn];
            availableSpawns.RemoveAt(indexSpawn);

            GameObject instance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation, transform);

            // obtener PassengerData
            PassengerData data = instance.GetComponent<PassengerData>();
            if (data == null)
            {
                Debug.LogWarning("[PassengerPlacementLogic] El prefab " + prefab.name + " no tiene PassengerData.");
            }
            else
            {
                // aplicar el debugColor al cubo
                Renderer r = instance.GetComponentInChildren<Renderer>();
                if (r != null)
                {
                    r.material.color = data.debugColor;
                }
            }

            instantiatedPassengers.Add(instance);
        }

        Debug.Log("[PassengerPlacementLogic] Se han generado " + instantiatedPassengers.Count + " pasajeros (minimo 2, rating " + (gameStats?.rating ?? 0) + ").");

        return instantiatedPassengers;
    }

    public int GetSpawnPointCount()
    {
        return spawnPoints != null ? spawnPoints.Count : 0;
    }
}