using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SecretCharacterUnlock : MonoBehaviour
{
    [Header("Galería de personajes")]
    public Image currentCharacterImage;
    public Sprite secretCharacterSprite;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip keySound;
    public AudioClip unlockSound;

    private string sequence = "HER";
    private string currentInput = "";

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.hKey.wasPressedThisFrame) RegisterKey('H');
        else if (Keyboard.current.eKey.wasPressedThisFrame) RegisterKey('E');
        else if (Keyboard.current.rKey.wasPressedThisFrame) RegisterKey('R');
        else if (Keyboard.current.anyKey.wasPressedThisFrame)
            ResetSequence();
    }

    void RegisterKey(char key)
    {
        if (audioSource && keySound)
            audioSource.PlayOneShot(keySound);

        currentInput += key;

        if (currentInput.Length > sequence.Length)
        {
            ResetSequence();
            return;
        }

        if (sequence.StartsWith(currentInput))
        {
            if (currentInput == sequence)
            {
                UnlockSecretCharacter();
                ResetSequence();
            }
        }
        else
        {
            ResetSequence();
        }
    }

    void ResetSequence() => currentInput = "";

    void UnlockSecretCharacter()
    {
        if (audioSource && unlockSound)
            audioSource.PlayOneShot(unlockSound);

        if (currentCharacterImage && secretCharacterSprite)
            currentCharacterImage.sprite = secretCharacterSprite;
    }
}
