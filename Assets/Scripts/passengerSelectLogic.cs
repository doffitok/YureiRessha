using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class passengerSelectLogic : MonoBehaviour
{
    [Header("Referencias")]
    public passengerPlacementLogic placementLogic;

    private void Start()
    {
        if (placementLogic == null)
            placementLogic = FindFirstObjectByType<passengerPlacementLogic>();

        if (placementLogic == null)
        {
            Debug.LogError("[passengerSelectLogic] No se encontró passengerPlacementLogic en la escena.");
            return;
        }

        StartCoroutine(DelayedSetup());
    }

    private IEnumerator DelayedSetup()
    {
        yield return null; // esperar 1 frame

        List<GameObject> prefabs = placementLogic.passengerPrefabs;

        if (prefabs == null || prefabs.Count == 0)
        {
            Debug.LogError("[passengerSelectLogic] No hay prefabs de pasajeros asignados. Se usarán personajes default.");
            prefabs = placementLogic.defaultPassengers;
            if (prefabs == null || prefabs.Count == 0)
            {
                Debug.LogError("[passengerSelectLogic] Tampoco hay personajes default disponibles.");
                yield break;
            }
        }

        int maxSpawn = placementLogic.GetSpawnPointCount();
        int numToSpawn = Mathf.Min(prefabs.Count, maxSpawn);

        // 🔹 Tiradas y separación en listas de prioridad
        List<(GameObject prefab, int roll, int demandaMin)> candidateRolls = new List<(GameObject, int, int)>();
        List<GameObject> prioritized = new List<GameObject>();
        List<GameObject> nonPrioritized = new List<GameObject>();

        foreach (GameObject candidate in prefabs)
        {
            itemIdentifier identifier = candidate.GetComponent<itemIdentifier>();
            int roll = 0;
            int demandaMin = 0;

            if (identifier != null && identifier.characterData != null)
            {
                var data = identifier.characterData;
                roll = Random.Range(data.demandaMin, data.demandaMax + 1);
                demandaMin = data.demandaMin;
            }

            candidateRolls.Add((candidate, roll, demandaMin));
        }

        // Mostrar todos los rolls
        Debug.Log("[passengerSelectLogic] Rolls de todos los candidatos:");
        foreach (var c in candidateRolls)
        {
            Debug.Log($"- {c.prefab.name}: Roll={c.roll}, DemandaMin={c.demandaMin}");
        }

        // Separar prioridades
        foreach (var c in candidateRolls)
        {
            if (c.demandaMin >= 40)
                prioritized.Add(c.prefab);
            else
                nonPrioritized.Add(c.prefab);
        }

        // Ordenar descendente según roll
        prioritized = prioritized.OrderByDescending(p => candidateRolls.First(c => c.prefab == p).roll).ToList();
        nonPrioritized = nonPrioritized.OrderByDescending(p => candidateRolls.First(c => c.prefab == p).roll).ToList();

        // Seleccionar hasta numToSpawn
        List<GameObject> finalSelection = new List<GameObject>();
        finalSelection.AddRange(prioritized);
        if (finalSelection.Count < numToSpawn)
            finalSelection.AddRange(nonPrioritized.Take(numToSpawn - finalSelection.Count));

        finalSelection = finalSelection.Take(numToSpawn).ToList();

        // Mostrar selección final
        Debug.Log("[passengerSelectLogic] Selección final de pasajeros:");
        foreach (var p in finalSelection)
        {
            Debug.Log($"- {p.name}");
        }

        // Instanciar
        placementLogic.SpawnPassengers(finalSelection);
    }
}