using UnityEngine;
using System.Collections.Generic;

public class PassengerPlacementLogic : MonoBehaviour
{
    [Header("Configuracion")]
    public Transform passengerSpawnsParent;        // 🔹 El Empty "passengerSpawns"
    public GameObject passengerPlaceholderPrefab;  // 🔹 El prefab (tu cubo)

    private List<Transform> spawnPoints = new List<Transform>();

    void Start()
    {
        // Guardamos todos los hijos de passengerSpawns
        foreach (Transform child in passengerSpawnsParent)
        {
            spawnPoints.Add(child);
        }

        // Apenas arranca el juego → spawneamos pasajeros
        SpawnPassengers();
    }

    // 🔹 Hacer público para poder llamarlo desde DebuggerMenu
    public void SpawnPassengers()
    {
        // Limpiamos cualquier passenger previo
        foreach (Transform spawn in spawnPoints)
        {
            foreach (Transform child in spawn)
            {
                Destroy(child.gameObject);
            }
        }

        // Número de pasajeros mínimo y máximo
        int minPassengers = 2;
        int maxPassengers = spawnPoints.Count;

        // Buscamos stats
        GameStats stats = FindFirstObjectByType<GameStats>();
        if (stats == null)
        {
            Debug.LogWarning("[PassengerPlacementLogic] No se encontro GameStats en la escena.");
            return;
        }

        // Empezamos con el mínimo garantizado
        int passengersToSpawn = minPassengers;

        // Asientos extra
        int extraSeats = maxPassengers - minPassengers;
        int ratingInternal = stats.rating; // Valor 0–60

        for (int i = 0; i < extraSeats; i++)
        {
            int roll = Random.Range(0, 61); // 0 a 60 inclusive
            if (roll <= ratingInternal)
            {
                passengersToSpawn++;
            }
        }

        // Lista temporal de spawns disponibles
        List<Transform> availableSpawns = new List<Transform>(spawnPoints);

        for (int i = 0; i < passengersToSpawn; i++)
        {
            int index = Random.Range(0, availableSpawns.Count);
            Transform chosenSpawn = availableSpawns[index];

            Instantiate(passengerPlaceholderPrefab, chosenSpawn.position, Quaternion.identity, chosenSpawn);
            availableSpawns.RemoveAt(index);
        }
    }
}