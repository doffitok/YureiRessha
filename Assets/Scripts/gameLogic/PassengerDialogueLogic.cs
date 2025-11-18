using UnityEngine;
using System.Collections;

public class PassengerDialogueLogic : MonoBehaviour
{
    public GameObject dialogueIconPrefab;

    public float offsetX = 0f;
    public float offsetY = 0f;
    public float offsetZ = 0f;

    public float minSpawnDelay = 4f;
    public float maxSpawnDelay = 8f;

    private GameObject currentIcon;
    private Coroutine spawnRoutine;

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

    private void SpawnIcon()
    {
        Vector3 spawnPos = transform.position + new Vector3(offsetX, offsetY, offsetZ);

        currentIcon = Instantiate(dialogueIconPrefab, spawnPos, Quaternion.identity, transform);

        currentIcon.AddComponent<DialogueIconBillboard>();

        // Asegurar collider 2D
        Collider2D col2D = currentIcon.GetComponent<Collider2D>();
        if (col2D == null)
        {
            col2D = currentIcon.AddComponent<BoxCollider2D>();
        }

        // Agregar comportamiento de click 2D
        DialogueIconClick2D click = currentIcon.AddComponent<DialogueIconClick2D>();
        click.owner = this;
    }

    public void OnIconClicked()
    {
        if (currentIcon != null)
        {
            Destroy(currentIcon);
            currentIcon = null;
        }
    }
}

class DialogueIconClick2D : MonoBehaviour
{
    public PassengerDialogueLogic owner;

    private void OnMouseUpAsButton()
    {
        if (owner != null)
            owner.OnIconClicked();

        Destroy(gameObject);
    }
}

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