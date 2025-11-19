using UnityEngine;
using System.Collections;
using Fungus;
using UnityEngine.EventSystems;

public class PassengerDialogueLogic : MonoBehaviour
{
    [Header("Prefab de la burbuja de dialogo (con BoxCollider2D)")]
    public GameObject dialogueIconPrefab;

    [Header("Offset del icono respecto al pasajero")]
    public float offsetX = 0f;
    public float offsetY = 0f;
    public float offsetZ = 0f;

    [Header("Tiempo aleatorio entre spawns")]
    public float minSpawnDelay = 4f;
    public float maxSpawnDelay = 8f;

    [Header("Fungus")]
    [Tooltip("Nombre del GameObject que tiene el Flowchart en la escena. Si esta vacio, se usara el primer Flowchart que se encuentre.")]
    public string flowchartName;
    [Tooltip("Nombre del Block que se ejecutara cuando se haga click en la burbuja.")]
    public string blockName;

    private GameObject currentIcon;
    private Coroutine spawnRoutine;
    private Flowchart cachedFlowchart;

    private void Start()
    {
        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);

            if (currentIcon == null)
                SpawnIcon();
        }
    }

    // Busca y cachea el Flowchart en la escena
    private Flowchart ResolveFlowchart()
    {
        if (cachedFlowchart != null)
            return cachedFlowchart;

        // 1) Buscar por nombre de GameObject si se especifico flowchartName
        if (!string.IsNullOrEmpty(flowchartName))
        {
            GameObject go = GameObject.Find(flowchartName);
            if (go != null)
            {
                cachedFlowchart = go.GetComponent<Flowchart>();
                if (cachedFlowchart != null)
                    return cachedFlowchart;

                Debug.LogWarning("[PassengerDialogueLogic] El GameObject '" + flowchartName + "' no tiene componente Flowchart.");
            }
            else
            {
                Debug.LogWarning("[PassengerDialogueLogic] No se encontro un GameObject llamado '" + flowchartName + "'.");
            }
        }

        // 2) Si no se encontro por nombre, usar el primer Flowchart de la escena
        cachedFlowchart = Object.FindObjectOfType<Flowchart>();
        if (cachedFlowchart == null)
        {
            Debug.LogWarning("[PassengerDialogueLogic] No se encontro ningun Flowchart en la escena.");
        }

        return cachedFlowchart;
    }

    private void SpawnIcon()
    {
        if (dialogueIconPrefab == null)
        {
            Debug.LogError("[PassengerDialogueLogic] dialogueIconPrefab no asignado.");
            return;
        }

        Vector3 spawnPos = transform.position + new Vector3(offsetX, offsetY, offsetZ);

        // Instanciamos el prefab tal cual, como hijo del pasajero
        currentIcon = Instantiate(dialogueIconPrefab, spawnPos, Quaternion.identity, transform);

        // Hacer que mire a la camara
        currentIcon.AddComponent<DialogueIconBillboard>();

        // Asegurarnos de que tiene Collider2D
        Collider2D col2D = currentIcon.GetComponent<Collider2D>();
        if (col2D == null)
        {
            Debug.LogError("[PassengerDialogueLogic] El prefab de burbuja NO tiene Collider2D. No se podra clickear.");
        }

        // Agregar manejador de click via sistema de eventos (Input System friendly)
        DialogueIconClick2D click = currentIcon.AddComponent<DialogueIconClick2D>();
        click.owner = this;
        click.flowchart = ResolveFlowchart();
        click.blockName = blockName;
    }

    // Llamado desde el icono cuando se hace click
    public void OnIconClicked()
    {
        if (currentIcon != null)
        {
            Destroy(currentIcon);
            currentIcon = null;
        }
    }
}

// --------------------------------------------------------------------
// Detecta el click usando el sistema de eventos (compatible con Input System)
// --------------------------------------------------------------------
class DialogueIconClick2D : MonoBehaviour, IPointerClickHandler
{
    public PassengerDialogueLogic owner;
    public Flowchart flowchart;
    public string blockName;

    public void OnPointerClick(PointerEventData eventData)
    {
        // Ejecutar dialogo de Fungus
        if (flowchart != null && !string.IsNullOrEmpty(blockName))
        {
            flowchart.ExecuteBlock(blockName);
        }
        else
        {
            Debug.Log("[DialogueIconClick2D] Click, pero flowchart o blockName no asignados.");
        }

        // Avisar al passenger para destruir la burbuja
        if (owner != null)
            owner.OnIconClicked();

        // Destruir el icono en si
        Destroy(gameObject);
    }
}

// --------------------------------------------------------------------
// Hace que el icono mire siempre a la camara
// --------------------------------------------------------------------
class DialogueIconBillboard : MonoBehaviour
{
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (cam != null)
        {
            transform.LookAt(
                transform.position + cam.transform.rotation * Vector3.forward,
                cam.transform.rotation * Vector3.up
            );
        }
    }
}