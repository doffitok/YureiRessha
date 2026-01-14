using UnityEngine;

[DisallowMultipleComponent]
public class EndScreenBehaviour : MonoBehaviour
{
    [Header("Prefab del Game Over (diario)")]
    public GameObject endScreenPrefab;

    [Header("HUD que se va a sacudir")]
    public RectTransform hudRoot;

    [Header("Shake del HUD")]
    public float shakeFuerza = 0.5f;
    public float shakeDuracion = 0.5f;

    [Header("Sonido de ruptura")]
    public AudioClip sonidoRuptura;
    public float volumenSonido = 1f;

    [Header("Animación del diario")]
    public Vector3 escalaInicial = Vector3.zero;
    public Vector3 escalaFinal = Vector3.one;
    public float vueltasCompletas = 2f;
    public float duracionAnimacion = 1.5f;

    private Canvas canvasPadre;
    private AudioSource audioSource;

    private bool jugadorPerdio = false;

    // Shake HUD
    private bool shaking = false;
    private float shakeTimer = 0f;
    private Vector2 hudOriginal;
    private bool shakeYaHecho = false;

    // Diario
    private RectTransform diarioRect;
    private bool animandoDiario = false;
    private float diarioTimer = 0f;
    private bool diarioYaMostrado = false;

    private void Awake()
    {
        canvasPadre = GetComponentInParent<Canvas>();
        if (canvasPadre == null)
            canvasPadre = FindFirstObjectByType<Canvas>();

        if (hudRoot == null && canvasPadre != null)
            hudRoot = canvasPadre.GetComponent<RectTransform>();

        if (hudRoot != null)
            hudOriginal = hudRoot.anchoredPosition;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    // Llamado desde el sistema de resultados
    public void SetResultadoDelDia(bool perdio)
    {
        jugadorPerdio = perdio;
        shakeYaHecho = false;
    }

    // Llamado cuando termina la animación del balance
    public void OnBalanceAnimationCompletada()
    {
        if (!jugadorPerdio || shakeYaHecho)
            return;

        TriggerShake();
        shakeYaHecho = true;
    }

    private void TriggerShake()
    {
        if (hudRoot == null)
            return;

        shakeTimer = 0f;
        shaking = true;

        if (sonidoRuptura != null)
        {
            audioSource.volume = volumenSonido;
            audioSource.PlayOneShot(sonidoRuptura);
        }
    }

    // Llamado para mostrar el diario de Game Over
    public void ActivarEndScreen()
    {
        if (!jugadorPerdio || diarioYaMostrado)
            return;

        InstanciarDiarioSiEsNecesario();
        if (diarioRect == null)
            return;

        diarioRect.localScale = escalaInicial;
        diarioRect.localRotation = Quaternion.identity;

        diarioTimer = 0f;
        animandoDiario = true;
        diarioYaMostrado = true;
    }

    private void InstanciarDiarioSiEsNecesario()
    {
        if (diarioRect != null)
            return;

        if (endScreenPrefab == null)
            return;

        if (canvasPadre == null)
            canvasPadre = FindFirstObjectByType<Canvas>();

        if (canvasPadre == null)
            return;

        GameObject instancia = Instantiate(endScreenPrefab, canvasPadre.transform);
        instancia.name = "EndScreenDiario";

        diarioRect = instancia.GetComponent<RectTransform>();
        if (diarioRect == null)
            return;

        diarioRect.anchorMin = new Vector2(0.5f, 0.5f);
        diarioRect.anchorMax = new Vector2(0.5f, 0.5f);
        diarioRect.pivot = new Vector2(0.5f, 0.5f);
        diarioRect.anchoredPosition = Vector2.zero;
    }

    private void Update()
    {
        ActualizarShake();
        ActualizarAnimacionDiario();
    }

    private void ActualizarShake()
    {
        if (!shaking || hudRoot == null)
            return;

        if (shakeTimer < shakeDuracion)
        {
            shakeTimer += Time.deltaTime;

            float x = Random.Range(-shakeFuerza, shakeFuerza);
            float y = Random.Range(-shakeFuerza, shakeFuerza);
            hudRoot.anchoredPosition = hudOriginal + new Vector2(x, y);
        }
        else
        {
            shaking = false;
            hudRoot.anchoredPosition = hudOriginal;
        }
    }

    private void ActualizarAnimacionDiario()
    {
        if (!animandoDiario || diarioRect == null)
            return;

        if (diarioTimer < duracionAnimacion)
        {
            diarioTimer += Time.deltaTime;

            float t = Mathf.Clamp01(diarioTimer / duracionAnimacion);
            diarioRect.localScale = Vector3.Lerp(escalaInicial, escalaFinal, t);

            float grados = vueltasCompletas * 360f * t;
            diarioRect.localRotation = Quaternion.Euler(0f, 0f, grados);
        }
        else
        {
            animandoDiario = false;
        }
    }
}
