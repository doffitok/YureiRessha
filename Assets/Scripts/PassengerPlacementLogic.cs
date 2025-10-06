using UnityEngine;
using System.Collections.Generic;

public class passengerPlacementLogic : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Empty que contiene los puntos de spawn de los pasajeros.")]
    public Transform passengerSpawnsParent;
    
    [Tooltip("Prefab del placeholder del pasajero.")]
    public GameObject passengerPlaceholderPrefab;
    
    private List<Transform> spawnPoints = new List<Transform>();
    [HideInInspector] public List<GameObject> activePassengers = new List<GameObject>();

    void Start()
    {
        InitializeSpawnPoints();
        SpawnPassengers();
    }

    private void InitializeSpawnPoints()
    {
        foreach (Transform child in passengerSpawnsParent)
        {
            spawnPoints.Add(child);
        }
    }

    public void SpawnPassengers()
    {
        // Limpiar pasajeros existentes
        foreach (GameObject passenger in activePassengers)
        {
            Destroy(passenger);
        }
        activePassengers.Clear();

        // Calcular número de pasajeros basado en rating
        GameStats stats = FindObjectOfType<GameStats>();
        int minPassengers = 2;
        int maxPassengers = spawnPoints.Count;
        int passengersToSpawn = minPassengers;

        if (stats != null)
        {
            int rating = Mathf.Clamp(stats.rating, 0, 60);
            int extraSeats = maxPassengers - minPassengers;
            
            for (int i = 0; i < extraSeats; i++)
            {
                if (Random.Range(0, 61) <= rating)
                {
                    passengersToSpawn++;
                }
            }
        }

        // Crear pasajeros
        List<Transform> availableSpawns = new List<Transform>(spawnPoints);
        for (int i = 0; i < passengersToSpawn; i++)
        {
            int index = Random.Range(0, availableSpawns.Count);
            Transform chosenSpawn = availableSpawns[index];
            GameObject placeholder = Instantiate(
                passengerPlaceholderPrefab,
                chosenSpawn.position,
                Quaternion.identity,
                chosenSpawn
            );
            availableSpawns.RemoveAt(index);
            activePassengers.Add(placeholder);
        }
    }

    public Transform[] GetPassengerPlaceholders()
    {
        Transform[] placeholders = new Transform[activePassengers.Count];
        for (int i = 0; i < activePassengers.Count; i++)
        {
            placeholders[i] = activePassengers[i].transform;
        }
        return placeholders;
    }
}