using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class AlmanacController : MonoBehaviour
{
    [Header("Data")]
    public List<CharacterAlmaData> characters = new List<CharacterAlmaData>();

    [Header("Buttons UI")]
    public Transform buttonsContainer;
    public GameObject buttonPrefab;

    [Header("ScrollView Fallback (opcional)")]
    public ScrollRect scrollView;

    [Header("Detail Panel UI (Left)")]
    public Image portraitImage;
    public Text titleText;
    public TextMeshProUGUI titleTMP;
    public Text descriptionText;
    public TextMeshProUGUI descriptionTMP;
    public Image[] galleryImages = new Image[3];

    [Header("Options")]
    public int defaultIndex = 0;

    [Header("Audio")]
    public AudioClip buttonSFX;
    public AudioSource audioSource;

    [Header("Button Size Options")]
    public Vector2 buttonSize = new Vector2(300f, 250f);

    [Header("Engimono Info Panel")]
    public GameObject engimonoPanel;
    public TextMeshProUGUI engimonoTitle;
    public TextMeshProUGUI engimonoDescription;
    public Image engimonoIcon;

    [Header("Engimono Slots")]
    public Image[] engimonoSlots = new Image[3];

    private string[] currentEngimonoNames = new string[3];
    private string[] currentEngimonoDescriptions = new string[3];
    private Sprite[] currentEngimonoIcons = new Sprite[3];

    [Range(0.1f, 100f)] public float scrollSpeed = 100f;
    private List<Button> spawnedButtons = new List<Button>();

    void Start()
    {
        GenerateCharacterButtons();
    }

    void Update()
    {
        if (scrollView != null)
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scrollInput) > 0.01f)
            {
                scrollView.verticalNormalizedPosition += scrollInput * scrollSpeed;
                scrollView.verticalNormalizedPosition = Mathf.Clamp01(scrollView.verticalNormalizedPosition);
            }
        }
    }

    public void GenerateCharacterButtons()
    {
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

        foreach (Transform child in buttonsContainer)
            Destroy(child.gameObject);
        spawnedButtons.Clear();

        for (int i = 0; i < characters.Count; i++)
        {
            int index = i;
            CharacterAlmaData c = characters[i];

            GameObject newButton = Instantiate(buttonPrefab);
            newButton.transform.SetParent(buttonsContainer, false);
            newButton.name = "AlmaButton_" + (string.IsNullOrEmpty(c.characterId) ? i.ToString() : c.characterId);

            RectTransform rect = newButton.GetComponent<RectTransform>();
            if (rect != null)
                rect.sizeDelta = buttonSize;

            Image background = newButton.GetComponent<Image>();
            if (background != null && c.buttonBackground != null)
            {
                background.sprite = c.buttonBackground;
                background.preserveAspect = true;
            }

            // 🟡 TÍTULO DESACTIVADO (se quita texto del botón)
            Text label = newButton.GetComponentInChildren<Text>();
            TextMeshProUGUI labelTMP = newButton.GetComponentInChildren<TextMeshProUGUI>();

            if (label != null)
                label.gameObject.SetActive(false);
            if (labelTMP != null)
                labelTMP.gameObject.SetActive(false);

            // Ícono del personaje
            Image[] imgs = newButton.GetComponentsInChildren<Image>();
            if (imgs != null && imgs.Length > 0 && c.portrait != null)
            {
                foreach (var img in imgs)
                {
                    if (img.gameObject == newButton) continue;
                    img.sprite = c.portrait;
                    img.preserveAspect = true;
                    break;
                }
            }

            // Click del botón
            Button btn = newButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ShowCharacter(index));
                btn.onClick.AddListener(() => {
                    if (audioSource != null && buttonSFX != null)
                        audioSource.PlayOneShot(buttonSFX);
                });
                spawnedButtons.Add(btn);
            }
        }

        if (characters.Count > 0)
            ShowCharacter(defaultIndex);

        if (buttonsContainer != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(buttonsContainer.GetComponent<RectTransform>());
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

        string nameToShow = string.IsNullOrEmpty(c.displayName) ? "—" : c.displayName;
        string descToShow = string.IsNullOrEmpty(c.description) ? "—" : c.description;

        if (titleText != null) titleText.text = nameToShow;
        if (titleTMP != null) titleTMP.text = nameToShow;

        if (descriptionText != null) descriptionText.text = descToShow;
        if (descriptionTMP != null) descriptionTMP.text = descToShow;

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

        // Engimonos
        if (c.engimonoNames != null)
            currentEngimonoNames = c.engimonoNames;
        if (c.engimonoDescriptions != null)
            currentEngimonoDescriptions = c.engimonoDescriptions;
        if (c.engimonoIcons != null)
            currentEngimonoIcons = c.engimonoIcons;

        for (int i = 0; i < engimonoSlots.Length; i++)
        {
            if (i < currentEngimonoIcons.Length && currentEngimonoIcons[i] != null)
            {
                engimonoSlots[i].sprite = currentEngimonoIcons[i];
                engimonoSlots[i].gameObject.SetActive(true);
                engimonoSlots[i].preserveAspect = true;

                var slot = engimonoSlots[i].GetComponent<EngimonoSlot>();
                if (slot == null) slot = engimonoSlots[i].gameObject.AddComponent<EngimonoSlot>();
                slot.Setup(i, this);
            }
            else
            {
                engimonoSlots[i].gameObject.SetActive(false);
            }
        }

        if (engimonoPanel != null)
            engimonoPanel.SetActive(false);

        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            if (spawnedButtons[i] == null) continue;
            ColorBlock colors = spawnedButtons[i].colors;
            colors.normalColor = (i == index) ? Color.white : Color.gray;
            spawnedButtons[i].colors = colors;
        }
    }

    public void ShowEngimonoInfo(int index)
    {
        if (engimonoPanel == null || index < 0 || index >= currentEngimonoNames.Length)
            return;

        engimonoTitle.text = currentEngimonoNames[index];
        engimonoDescription.text = currentEngimonoDescriptions[index];
        if (engimonoIcon != null)
            engimonoIcon.sprite = currentEngimonoIcons[index];

        engimonoPanel.SetActive(true);
    }

    public void HideEngimonoInfo()
    {
        if (engimonoPanel != null)
            engimonoPanel.SetActive(false);
    }

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
