using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // 🆕 Necesario para usar TextMeshPro

public class AlmanacController : MonoBehaviour
{
    [Header("Data")]
    public List<CharacterAlmaData> characters = new List<CharacterAlmaData>();

    [Header("Buttons UI")]
    public Transform buttonsContainer; // Content del ScrollView o un panel con Vertical Layout Group
    public GameObject buttonPrefab;    // prefab del botón (Button + Text y/o Image)

    [Header("ScrollView Fallback (opcional)")]
    public ScrollRect scrollView; // 🆕 Si no asignas el container, se usará el Content de este ScrollView

    [Header("Detail Panel UI (Left)")]
    public Image portraitImage;        // imagen grande
    public Text titleText;             // título (UI Text)
    public TextMeshProUGUI titleTMP;   // 🆕 soporte opcional TMP
    public Text descriptionText;       // descripción (UI Text)
    public TextMeshProUGUI descriptionTMP; // 🆕 soporte opcional TMP
    public Image[] galleryImages = new Image[3]; // 3 miniaturas

    [Header("Options")]
    public int defaultIndex = 0;

    private List<Button> spawnedButtons = new List<Button>();


    void Start()
    {
        GenerateCharacterButtons();
    }

    // 🔹 Genera los botones del almanaque
    public void GenerateCharacterButtons()
    {
        // 🆕 Fallback automático: si no se asignó el container, lo tomamos del ScrollView
        if (buttonsContainer == null && scrollView != null && scrollView.content != null)
        {
            buttonsContainer = scrollView.content;
            Debug.Log("🧭 buttonsContainer no asignado — se usará el Content del ScrollView.");
        }

        if (buttonsContainer == null || buttonPrefab == null)
        {
            Debug.LogWarning("❌ Falta asignar buttonsContainer o buttonPrefab en el inspector.");
            return;
        }

        // 🔸 Limpia botones anteriores
        foreach (Transform child in buttonsContainer)
            Destroy(child.gameObject);
        spawnedButtons.Clear();

        // 🔸 Genera uno nuevo por cada personaje
        for (int i = 0; i < characters.Count; i++)
        {
            int index = i;
            CharacterAlmaData c = characters[i];

            // Instancia el botón dentro del ScrollView Content
            GameObject newButton = Instantiate(buttonPrefab);
            newButton.transform.SetParent(buttonsContainer, false); // false evita mantener posiciones locales raras
            newButton.name = "AlmaButton_" + (string.IsNullOrEmpty(c.characterId) ? i.ToString() : c.characterId);

            // 🆕 Soporte Text o TextMeshPro
            Text label = newButton.GetComponentInChildren<Text>();
            TextMeshProUGUI labelTMP = newButton.GetComponentInChildren<TextMeshProUGUI>();
            string displayName = string.IsNullOrEmpty(c.displayName) ? "Unnamed" : c.displayName;
            if (label != null) label.text = displayName;
            if (labelTMP != null) labelTMP.text = displayName;

            // Asigna ícono (opcional)
            Image[] imgs = newButton.GetComponentsInChildren<Image>();
            if (imgs != null && imgs.Length > 0 && c.portrait != null)
            {
                foreach (var img in imgs)
                {
                    if (img.gameObject == newButton) continue; // evitar fondo del botón
                    img.sprite = c.portrait;
                    img.preserveAspect = true;
                    break;
                }
            }

            // Configura evento del botón
            Button btn = newButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ShowCharacter(index));
                spawnedButtons.Add(btn);
            }
        }

        // 🔸 Mostrar el primer personaje por defecto (opcional)
        if (characters.Count > 0)
            ShowCharacter(defaultIndex);

        // 🔸 Forzar actualización del layout (para ScrollView)
        if (buttonsContainer != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(buttonsContainer.GetComponent<RectTransform>());
    }


    // 🔹 Muestra la información del personaje seleccionado
    public void ShowCharacter(int index)
    {
        if (characters == null || characters.Count == 0)
        {
            ClearDetail();
            return;
        }

        if (index < 0 || index >= characters.Count)
        {
            ClearDetail();
            return;
        }

        CharacterAlmaData c = characters[index];

        // Retrato
        if (portraitImage != null)
        {
            if (c.portrait != null)
            {
                portraitImage.sprite = c.portrait;
                portraitImage.gameObject.SetActive(true);
                portraitImage.preserveAspect = true;
            }
            else portraitImage.gameObject.SetActive(false);
        }

        // 🆕 Soporte Text y TMP
        string nameToShow = string.IsNullOrEmpty(c.displayName) ? "—" : c.displayName;
        string descToShow = string.IsNullOrEmpty(c.description) ? "—" : c.description;

        if (titleText != null) titleText.text = nameToShow;
        if (titleTMP != null) titleTMP.text = nameToShow;

        if (descriptionText != null) descriptionText.text = descToShow;
        if (descriptionTMP != null) descriptionTMP.text = descToShow;

        // Galería
        for (int i = 0; i < galleryImages.Length; i++)
        {
            if (galleryImages[i] == null) continue;
            if (c.gallerySprites != null && i < c.gallerySprites.Length && c.gallerySprites[i] != null)
            {
                galleryImages[i].sprite = c.gallerySprites[i];
                galleryImages[i].gameObject.SetActive(true);
                galleryImages[i].preserveAspect = true;
            }
            else
            {
                galleryImages[i].gameObject.SetActive(false);
            }
        }

        // Resalta el botón activo
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            if (spawnedButtons[i] == null) continue;
            ColorBlock colors = spawnedButtons[i].colors;
            colors.normalColor = (i == index) ? Color.white : Color.gray;
            spawnedButtons[i].colors = colors;
        }
    }


    // 🔹 Limpia la UI si no hay personaje
    void ClearDetail()
    {
        if (portraitImage != null) portraitImage.gameObject.SetActive(false);

        if (titleText != null) titleText.text = "";
        if (titleTMP != null) titleTMP.text = "";

        if (descriptionText != null) descriptionText.text = "";
        if (descriptionTMP != null) descriptionTMP.text = "";

        foreach (var img in galleryImages)
            if (img != null) img.gameObject.SetActive(false);
    }
}
