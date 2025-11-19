using UnityEngine;

[DisallowMultipleComponent]
public class EndScreenBehaviour : MonoBehaviour
{
    [Header("Prefab del Game Over")]
    [SerializeField] private GameObject endScreenPrefab;

    [Header("HUD que se va a sacudir")]
    [SerializeField] private RectTransform hudRoot;

    [Header("Shake del HUD")]
    [SerializeField] private float shakeFuerza = 15f;
    [SerializeField] private float shakeDuracion = 0.5f;

    [Header("Sonido de ruptura")]
    [SerializeField] private AudioClip sonidoRuptura;
    [SerializeField] private float volumenSonido = 1f;

    [Header("Animacion del Game Over")]
    [SerializeField] private Vector3 escalaInicial = Vector3.zero;
    [SerializeField] private Vector3 escalaFinal = new Vector3(1f, 1f, 1f);
    [SerializeField] private float vueltasCompletas = 2f;
    [SerializeField] private float duracionAnimacion = 1.5f;

    private RectTransform endScreenRect;
    private Canvas canvasPadre;

    private AudioSource audioSource;

    private float animTimer = 0f;
    private bool animandoEndScreen = false;

    private bool shaking = false;
    private float shakeTimer = 0f;
    private Vector2 hudPosOriginal;

    private void Awake()
    {
        canvasPadre = GetComponentInParent<Canvas>();
        if (canvasPadre == null)
            canvasPadre = FindAnyObjectByType<Canvas>();

        if (hudRoot == null && canvasPadre != null)
            hudRoot = canvasPadre.GetComponent<RectTransform>();

        if (hudRoot != null)
            hudPosOriginal = hudRoot.anchoredPosition;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    // Llamar apenas termines de mostrar el balance (SI perdio)
    public void TriggerShake()
    {
        if (hudRoot == null)
        {
            Debug.LogWarning("[EndScreenBehaviour] No hay HUD asignado para shake.");
            return;
        }

        shakeTimer = 0f;
        shaking = true;

        if (sonidoRuptura != null)
        {
            audioSource.volume = volumenSonido;
            audioSource.PlayOneShot(sonidoRuptura);
        }

        Debug.Log("[EndScreenBehaviour] Shake activado.");
    }

    // Llamar despues en el cierre final para mostrar el diario
    public void ActivarEndScreen()
    {
        InstanciarEndScreenSiEsNecesario();

        if (endScreenRect == null)
            return;

        animTimer = 0f;
        endScreenRect.localScale = escalaInicial;
        endScreenRect.localRotation = Quaternion.identity;

        animandoEndScreen = true;

        Debug.Log("[EndScreenBehaviour] Animacion del diario iniciada.");
    }

    private void InstanciarEndScreenSiEsNecesario()
    {
        if (endScreenPrefab == null)
        {
            Debug.LogError("[EndScreenBehaviour] No hay prefab asignado.");
            return;
        }

        if (endScreenRect != null)
            return;

        GameObject instancia = Object.Instantiate(endScreenPrefab, canvasPadre.transform);
        instancia.name = "EndScreenInstancia";

        endScreenRect = instancia.GetComponent<RectTransform>();
        if (endScreenRect == null)
        {
            Debug.LogError("[EndScreenBehaviour] El prefab no tiene RectTransform.");
            return;
        }

        instancia.SetActive(true);

        endScreenRect.anchorMin = new Vector2(0.5f, 0.5f);
        endScreenRect.anchorMax = new Vector2(0.5f, 0.5f);
        endScreenRect.pivot = new Vector2(0.5f, 0.5f);
        endScreenRect.anchoredPosition = Vector2.zero;

        Debug.Log("[EndScreenBehaviour] Instancia del diario creada.");
    }

    private void Update()
    {
        ActualizarShake();
        ActualizarAnimacion();
    }

    private void ActualizarShake()
    {
        if (!shaking || hudRoot == null)
            return;

        if (shakeTimer < shakeDuracion)
        {
            shakeTimer += Time.deltaTime;

            float offsetX = Random.Range(-shakeFuerza, shakeFuerza);
            float offsetY = Random.Range(-shakeFuerza, shakeFuerza);

            hudRoot.anchoredPosition = hudPosOriginal + new Vector2(offsetX, offsetY);
        }
        else
        {
            hudRoot.anchoredPosition = hudPosOriginal;
            shaking = false;
        }
    }

    private void ActualizarAnimacion()
    {
        if (!animandoEndScreen || endScreenRect == null)
            return;

        if (animTimer < duracionAnimacion)
        {
            animTimer += Time.deltaTime;
            float t = Mathf.Clamp01(animTimer / duracionAnimacion);

            endScreenRect.localScale = Vector3.Lerp(escalaInicial, escalaFinal, t);

            float grados = vueltasCompletas * 360f * t;
            endScreenRect.localRotation = Quaternion.Euler(0f, 0f, grados);
        }
        else
        {
            animandoEndScreen = false;
        }
    }
}