using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class EngimonoGalleryController : MonoBehaviour
{
    [Header("Engimonos Data")]
    public List<EngimonoData> engimonos = new List<EngimonoData>();

    [Header("UI References")]
    public Transform gridContainer;       // Content del ScrollView
    public GameObject engimonoCardPrefab; // Prefab del botón de cada Engimono

    [Header("Info Panel")]
    public GameObject engimonoPanel;      // Panel de información
    public TextMeshProUGUI engimonoTitle;
    public TextMeshProUGUI engimonoDescription;
    public Image engimonoIcon;            // Imagen principal del Info Panel

    void Start()
    {
        PopulateGallery();
        if (engimonoPanel != null)
            engimonoPanel.SetActive(false);
    }

    void PopulateGallery()
    {
        // Limpiar la galería antes de llenarla
        foreach (Transform child in gridContainer)
            Destroy(child.gameObject);

        // Crear un botón por cada Engimono
        foreach (var e in engimonos)
        {
            GameObject card = Instantiate(engimonoCardPrefab, gridContainer);

            // Asignar el script de tarjeta
            EngimonoCard cardScript = card.GetComponent<EngimonoCard>();
            if (cardScript != null)
                cardScript.SetEngimono(e);

            // Configurar eventos de hover
            EventTrigger trigger = card.GetComponent<EventTrigger>();
            if (trigger == null) trigger = card.AddComponent<EventTrigger>();

            // Al pasar el mouse
            EventTrigger.Entry entryEnter = new EventTrigger.Entry();
            entryEnter.eventID = EventTriggerType.PointerEnter;
            entryEnter.callback.AddListener((data) => { ShowEngimono(e); });
            trigger.triggers.Add(entryEnter);

            // Al salir el mouse
            EventTrigger.Entry entryExit = new EventTrigger.Entry();
            entryExit.eventID = EventTriggerType.PointerExit;
            entryExit.callback.AddListener((data) => { HideEngimonoPanel(); });
            trigger.triggers.Add(entryExit);
        }
    }

    public void ShowEngimono(EngimonoData e)
    {
        if (engimonoPanel == null) return;

        engimonoTitle.text = e.engimonoName;
        engimonoDescription.text = e.engimonoDescription;

        // Mostrar la imagen principal (si existe) o el icono
        engimonoIcon.sprite = e.engimonoMainSprite != null ? e.engimonoMainSprite : e.engimonoIcon;

        engimonoPanel.SetActive(true);
    }

    public void HideEngimonoPanel()
    {
        if (engimonoPanel != null)
            engimonoPanel.SetActive(false);
    }
}
