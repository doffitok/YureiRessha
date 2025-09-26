using UnityEngine;
using System.Collections.Generic;

public class PassengerPlacementLogic : MonoBehaviour
{
    [Header("Configuración")]
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

    private void SpawnPassengers()
    {
        // Limpiamos cualquier passenger previo (por si acaso)
        foreach (Transform spawn in spawnPoints)
        {
            foreach (Transform child in spawn)
            {
                Destroy(child.gameObject);
            }
        }

        // Número aleatorio de pasajeros (entre 2 y 6)
        int passengersToSpawn = Random.Range(2, spawnPoints.Count + 1);

        // Lista temporal de spawns disponibles
        List<Transform> availableSpawns = new List<Transform>(spawnPoints);

        for (int i = 0; i < passengersToSpawn; i++)
        {
            // Escogemos un spawn aleatorio
            int index = Random.Range(0, availableSpawns.Count);
            Transform chosenSpawn = availableSpawns[index];

            // Creamos el pasajero en esa posición
            Instantiate(passengerPlaceholderPrefab, chosenSpawn.position, Quaternion.identity, chosenSpawn);

            // Quitamos ese spawn para no repetirlo
            availableSpawns.RemoveAt(index);
        }
    }
}