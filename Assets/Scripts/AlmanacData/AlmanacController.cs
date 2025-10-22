using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlmanacController : MonoBehaviour
{
    [Header("Data")]
    public List<CharacterAlmaData> characters = new List<CharacterAlmaData>();

    [Header("Buttons UI")]
    public Transform buttonsContainer; // Content del ScrollView o un panel con Vertical Layout Group
    public GameObject buttonPrefab;    // prefab del botón (Button + Text y/o Image)

    [Header("Detail Panel UI (Left)")]
    public Image portraitImage;        // imagen grande
    public Text titleText;             // título
    public Text descriptionText;       // descripción (Text o TextMeshPro)
    public Image[] galleryImages = new Image[3]; // 3 miniaturas

    [Header("Options")]
    public int defaultIndex = 0;

    [Header("Botones")]
    public Transform buttonContainer; // Panel donde van los botones
    public Button characterButtonPrefab; // Prefab del botón
    private List<Button> spawnedButtons = new List<Button>();

    void Start()
    {
        GenerateCharacterButtons();
    }

    void GenerateCharacterButtons()
    {
        // Limpia botones previos si ya existían
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedButtons.Clear();

        for (int i = 0; i < characters.Count; i++)
        {
            int index = i; // 👈 importante para que el listener no use la misma variable

            Button newButton = Instantiate(characterButtonPrefab, buttonContainer);
            spawnedButtons.Add(newButton);

            // Cambia el texto del botón
            Text btnText = newButton.GetComponentInChildren<Text>();
            if (btnText != null)
                btnText.text = characters[i].displayName;

            // Agrega listener al botón
            newButton.onClick.AddListener(() => ShowCharacter(index));
        }
    }

    void BuildButtons()
    {
        if (buttonsContainer == null || buttonPrefab == null) return;

        // limpiar hijos previos (modo Play)
        for (int i = buttonsContainer.childCount - 1; i >= 0; i--)
            Destroy(buttonsContainer.GetChild(i).gameObject);
        spawnedButtons.Clear();

        for (int i = 0; i < characters.Count; i++)
        {
            int index = i; // captura local
            CharacterAlmaData c = characters[i];

            GameObject go = Instantiate(buttonPrefab, buttonsContainer);
            go.name = "AlmaButton_" + (string.IsNullOrEmpty(c.characterId) ? i.ToString() : c.characterId);

            Button btn = go.GetComponent<Button>();
            if (btn == null)
            {
                Debug.LogWarning("buttonPrefab no tiene componente Button.");
                continue;
            }
            spawnedButtons.Add(btn);

            // label: busca Text hijo y asigna nombre
            Text label = go.GetComponentInChildren<Text>();
            if (label != null) label.text = string.IsNullOrEmpty(c.displayName) ? "Unnamed" : c.displayName;

            // icon: intenta asignar un Image hijo (no el background)
            Image[] imgs = go.GetComponentsInChildren<Image>();
            if (imgs != null && imgs.Length > 0 && c.portrait != null)
            {
                foreach (var img in imgs)
                {
                    if (img.gameObject == go) continue;
                    img.sprite = c.portrait;
                    img.preserveAspect = true;
                    break;
                }
            }

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => ShowCharacter(index));
        }
    }

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

        // portrait
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

        // title & description
        if (titleText != null) titleText.text = string.IsNullOrEmpty(c.displayName) ? "—" : c.displayName;
        if (descriptionText != null) descriptionText.text = string.IsNullOrEmpty(c.description) ? "—" : c.description;

        // gallery
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

        // highlight simple: colorea botones (opcional)
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            if (spawnedButtons[i] == null) continue;
            ColorBlock colors = spawnedButtons[i].colors;
            colors.normalColor = (i == index) ? Color.white : Color.gray;
            spawnedButtons[i].colors = colors;
        }
    }

    void ClearDetail()
    {
        if (portraitImage != null) portraitImage.gameObject.SetActive(false);
        if (titleText != null) titleText.text = "";
        if (descriptionText != null) descriptionText.text = "";
        foreach (var img in galleryImages)
            if (img != null) img.gameObject.SetActive(false);
    }
}
