using UnityEngine;

[DisallowMultipleComponent]
public class EndScreenBehaviour : MonoBehaviour
{
    [Header("Prefab del Game Over (diario)")]
    [SerializeField] private GameObject endScreenPrefab;

    [Header("HUD que se va a sacudir")]
    [SerializeField] private RectTransform hudRoot;

    [Header("Shake del HUD")]
    [SerializeField] private float shakeFuerza = 0.5f;
    [SerializeField] private float shakeDuracion = 0.5f;

    [Header("Sonido de ruptura")]
    [SerializeField] private AudioClip sonidoRuptura;
    [SerializeField] private float volumenSonido = 1f;

    [Header("Animacion del diario")]
    [SerializeField] private Vector3 escalaInicial = Vector3.zero;
    [SerializeField] private Vector3 escalaFinal = Vector3.one;
    [SerializeField] private float vueltasCompletas = 2f;
    [SerializeField] private float duracionAnimacion = 1.5f;

    // Nuevo: Sprite de fondo para Game Over
    [Header("Sprite de fondo Game Over")]
    [SerializeField] private GameObject fondoGameOverSprite; // Asigna el GameObject con el SpriteRenderer o Image
    [SerializeField] private bool mostrarFondoInmediatamente = true; // Mostrar al detectar game over

    private Canvas canvasPadre;
    private AudioSource audioSource;

    private bool jugadorPerdio = false;

    // Shake
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

        // Ocultar el sprite de fondo al inicio
        if (fondoGameOverSprite != null)
        {
            fondoGameOverSprite.SetActive(false);
        }

        Debug.Log("[EndScreenBehaviour] Awake completado.");
    }

    // GoalsManager llama esto despues de calcular el dia
    public void SetResultadoDelDia(bool perdio)
    {
        jugadorPerdio = perdio;
        shakeYaHecho = false;
        
        // Mostrar sprite de fondo si el jugador perdió
        if (jugadorPerdio && mostrarFondoInmediatamente && fondoGameOverSprite != null)
        {
            fondoGameOverSprite.SetActive(true);
            Debug.Log("[EndScreenBehaviour] Sprite de fondo Game Over activado.");
        }

        Debug.Log("[EndScreenBehaviour] SetResultadoDelDia llamado. Perdio=" + jugadorPerdio);
    }

    // EndscreenResults llama esto cuando TERMINA la animacion del balance
    public void OnBalanceAnimationCompletada()
    {
        Debug.Log("[EndScreenBehaviour] OnBalanceAnimationCompletada llamado.");

        if (!jugadorPerdio)
        {
            Debug.Log("[EndScreenBehaviour] El jugador NO perdio, no se hace shake.");
            return;
        }

        if (shakeYaHecho)
        {
            Debug.Log("[EndScreenBehaviour] Shake ya se hizo antes, se ignora.");
            return;
        }

        TriggerShake();
        shakeYaHecho = true;
    }

    private void TriggerShake()
    {
        if (hudRoot == null)
        {
            Debug.LogWarning("[EndScreenBehaviour] hudRoot es null, no se puede sacudir HUD.");
            return;
        }

        Debug.Log("[EndScreenBehaviour] TriggerShake: comenzando temblor + sonido.");
        shakeTimer = 0f;
        shaking = true;

        if (sonidoRuptura != null)
        {
            audioSource.volume = volumenSonido;
            audioSource.PlayOneShot(sonidoRuptura);
        }

        // Mostrar sprite de fondo si no se mostró inmediatamente
        if (!mostrarFondoInmediatamente && fondoGameOverSprite != null)
        {
            fondoGameOverSprite.SetActive(true);
            Debug.Log("[EndScreenBehaviour] Sprite de fondo Game Over activado durante shake.");
        }
    }

    public void ActivarEndScreen()
    {
        Debug.Log("[EndScreenBehaviour] ActivarEndScreen llamado. Perdio=" + jugadorPerdio + ", diarioYaMostrado=" + diarioYaMostrado);

        if (!jugadorPerdio)
        {
            Debug.Log("[EndScreenBehaviour] El jugador no perdio, no se muestra diario.");
            return;
        }

        if (diarioYaMostrado)
        {
            Debug.Log("[EndScreenBehaviour] El diario ya fue mostrado, se ignora.");
            return;
        }

        InstanciarDiarioSiEsNecesario();
        if (diarioRect == null)
        {
            Debug.LogError("[EndScreenBehaviour] No se pudo instanciar el diario.");
            return;
        }

        diarioRect.localScale = escalaInicial;
        diarioRect.localRotation = Quaternion.identity;

        diarioTimer = 0f;
        animandoDiario = true;
        diarioYaMostrado = true;

        // Asegurar que el sprite de fondo esté visible
        if (fondoGameOverSprite != null && !fondoGameOverSprite.activeSelf)
        {
            fondoGameOverSprite.SetActive(true);
        }

        Debug.Log("[EndScreenBehaviour] Animacion del diario iniciada.");
    }

    private void InstanciarDiarioSiEsNecesario()
    {
        if (diarioRect != null)
            return;

        if (endScreenPrefab == null)
        {
            Debug.LogError("[EndScreenBehaviour] endScreenPrefab es null, no puedo instanciar.");
            return;
        }

        if (canvasPadre == null)
        {
            canvasPadre = FindFirstObjectByType<Canvas>();
            if (canvasPadre == null)
            {
                Debug.LogError("[EndScreenBehaviour] No hay Canvas en la escena.");
                return;
            }
        }

        GameObject instancia = Instantiate(endScreenPrefab, canvasPadre.transform);
        instancia.name = "EndScreenDiario";

        diarioRect = instancia.GetComponent<RectTransform>();
        if (diarioRect == null)
        {
            Debug.LogError("[EndScreenBehaviour] El prefab no tiene RectTransform.");
            return;
        }

        diarioRect.anchorMin = new Vector2(0.5f, 0.5f);
        diarioRect.anchorMax = new Vector2(0.5f, 0.5f);
        diarioRect.pivot = new Vector2(0.5f, 0.5f);
        diarioRect.anchoredPosition = Vector2.zero;

        Debug.Log("[EndScreenBehaviour] Diario instanciado y centrado.");
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
            Debug.Log("[EndScreenBehaviour] Shake terminado.");
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
            Debug.Log("[EndScreenBehaviour] Animacion del diario terminada.");
        }
    }

    // Método público para ocultar el sprite si es necesario
    public void OcultarFondoGameOver()
    {
        if (fondoGameOverSprite != null)
        {
            fondoGameOverSprite.SetActive(false);
            Debug.Log("[EndScreenBehaviour] Sprite de fondo Game Over ocultado.");
        }
    }
}