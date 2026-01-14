using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PassengerSelectLogic : MonoBehaviour
{
    [Header("Referencias")]
    public PassengerPlacementLogic placementLogic;

    [Header("Ejecucion")]
    public bool autoRunOnStart = false;

    private bool hasRun = false;

    private void Awake()
    {
        if (placementLogic == null)
            placementLogic = FindFirstObjectByType<PassengerPlacementLogic>();
    }

    private void Start()
    {
        if (autoRunOnStart)
        {
            Debug.Log("[PassengerSelectLogic] autoRunOnStart TRUE → ejecutando seleccion al inicio.");
            RunSelectionSafe();
        }
    }

    public void RunSelectionSafe()
    {
        if (hasRun)
        {
            Debug.Log("[PassengerSelectLogic] RunSelectionSafe llamado pero ya corrio este ciclo.");
            return;
        }

        if (placementLogic == null)
            placementLogic = FindFirstObjectByType<PassengerPlacementLogic>();

        if (placementLogic == null)
        {
            Debug.LogError("[PassengerSelectLogic] No se encontro PassengerPlacementLogic en la escena.");
            return;
        }

        StartCoroutine(DelayedSetup());
    }

    public void ResetSelectionState()
    {
        hasRun = false;
    }

    private IEnumerator DelayedSetup()
    {
        hasRun = true;

        yield return null;

        List<GameObject> prefabs = placementLogic.passengerPrefabs;

        if (prefabs == null || prefabs.Count == 0)
        {
            Debug.LogWarning("[PassengerSelectLogic] No hay prefabs asignados. Usando default.");
            prefabs = placementLogic.defaultPassengers;

            if (prefabs == null || prefabs.Count == 0)
            {
                Debug.LogError("[PassengerSelectLogic] No hay personajes default disponibles.");
                yield break;
            }
        }

        int maxSpawn = placementLogic.GetSpawnPointCount();
        int numToSpawn = Mathf.Min(prefabs.Count, maxSpawn);

        List<(GameObject prefab, int roll, int demandaMin)> candidateRolls = new List<(GameObject, int, int)>();
        List<GameObject> prioritized = new List<GameObject>();
        List<GameObject> nonPrioritized = new List<GameObject>();

        foreach (GameObject candidate in prefabs)
        {
            PassengerData data = candidate.GetComponent<PassengerData>();

            int roll = 0;
            int demandaMin = 0;

            if (data != null)
            {
                roll = Random.Range(data.demandaMin, data.demandaMax + 1);
                demandaMin = data.demandaMin;
            }

            candidateRolls.Add((candidate, roll, demandaMin));
        }

        Debug.Log("[PassengerSelectLogic] Rolls generados:");
        foreach (var c in candidateRolls)
            Debug.Log("- " + c.prefab.name + ": Roll=" + c.roll + ", DemandaMin=" + c.demandaMin);

        foreach (var c in candidateRolls)
        {
            if (c.demandaMin >= 40)
                prioritized.Add(c.prefab);
            else
                nonPrioritized.Add(c.prefab);
        }

        prioritized = prioritized.OrderByDescending(p => candidateRolls.First(c => c.prefab == p).roll).ToList();
        nonPrioritized = nonPrioritized.OrderByDescending(p => candidateRolls.First(c => c.prefab == p).roll).ToList();

        List<GameObject> finalSelection = new List<GameObject>();
        finalSelection.AddRange(prioritized);

        if (finalSelection.Count < numToSpawn)
            finalSelection.AddRange(nonPrioritized.Take(numToSpawn - finalSelection.Count));

        finalSelection = finalSelection.Take(numToSpawn).ToList();

        Debug.Log("[PassengerSelectLogic] Seleccion final:");
        foreach (var p in finalSelection)
            Debug.Log("- " + p.name);

        placementLogic.SpawnPassengers(finalSelection);

        Debug.Log("[PassengerSelectLogic] Spawn solicitado de " + finalSelection.Count + " pasajeros.");
    }
}