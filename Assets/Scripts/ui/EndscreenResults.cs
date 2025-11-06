using UnityEngine;
using UnityEngine.InputSystem; // ✅ Nuevo Input System
using TMPro;
using System.Collections;

[DisallowMultipleComponent]
public class EndscreenResults : MonoBehaviour
{
    [Header("Referencias principales")]
    [SerializeField] private DayLogic dayLogic;
    [SerializeField] private GoalsManager goalsManager;

    [Header("UI de resultados")]
    [SerializeField] private GameObject panelResultados;
    [SerializeField] private GameObject grupoIngresos;
    [SerializeField] private GameObject grupoImpuestos;
    [SerializeField] private GameObject grupoBalance;
    [SerializeField] private TextMeshProUGUI textoIngresos;
    [SerializeField] private TextMeshProUGUI textoImpuestos;
    [SerializeField] private TextMeshProUGUI textoBalance;

    [Header("Tiempos generales")]
    [SerializeField] private float delayInicio = 1f;
    [SerializeField] private float tiempoEntreGrupos = 0.5f;
    [SerializeField] private float tiempoEntreAnimaciones = 0.75f;
    [SerializeField] private float delayAntesDeCalcular = 0.75f;

    [Header("Animación de Ingresos")]
    [SerializeField] private float segundosPorMil = 2f;
    [SerializeField] private float escalaExtra = 0.05f;
    [SerializeField] private float duracionMaximaIngresos = 8f;
    [Range(0f, 1f)] [SerializeField] private float porcentajeEscalaFinalIngresos = 0.5f;

    [Header("Rebote final de Ingresos")]
    [SerializeField] private float reboteEscalaFactor = 0.9f; // 🔹 cuánto se encoge (0.9 = 90%)
    [SerializeField] private float reboteDuracion = 0.25f;    // 🔹 duración del rebote

    [Header("Animación de Balance")]
    [SerializeField] private float duracionAleatoriaBalance = 3f;
    [SerializeField] private float rangoBalanceAleatorio = 3000f;
    [SerializeField] private float escalaBalanceDuranteAnimacion = 0.1f;
    [SerializeField] private float duracionRegresoBalance = 0.4f;

    private enum EstadoPantalla { Inactiva, MostrandoGrupos, MostrandoIngresos, MostrandoBalance, Finalizada }
    private EstadoPantalla estadoActual = EstadoPantalla.Inactiva;
    private bool skipSolicitado = false;

    private Vector3 escalaBaseIngresos;
    private Vector3 escalaBaseImpuestos;
    private Vector3 escalaBaseBalance;

    private void Start()
    {
        if (dayLogic == null)
            dayLogic = FindFirstObjectByType<DayLogic>();
        if (goalsManager == null)
            goalsManager = FindFirstObjectByType<GoalsManager>();

        if (dayLogic != null)
        {
            dayLogic.OnDayEnded += MostrarResultados;
            dayLogic.OnDayReset += OcultarResultados;
        }

        if (panelResultados != null)
            panelResultados.SetActive(false);

        if (textoIngresos != null) escalaBaseIngresos = textoIngresos.rectTransform.localScale;
        if (textoImpuestos != null) escalaBaseImpuestos = textoImpuestos.rectTransform.localScale;
        if (textoBalance != null) escalaBaseBalance = textoBalance.rectTransform.localScale;
    }

    private void OnDestroy()
    {
        if (dayLogic != null)
        {
            dayLogic.OnDayEnded -= MostrarResultados;
            dayLogic.OnDayReset -= OcultarResultados;
        }
    }

    private void Update()
    {
        if (panelResultados != null && panelResultados.activeSelf &&
            Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            skipSolicitado = true;
        }
    }

    private void MostrarResultados()
    {
        StartCoroutine(MostrarSecuencia());
    }

    private void OcultarResultados()
    {
        StopAllCoroutines();
        estadoActual = EstadoPantalla.Inactiva;

        if (panelResultados != null)
            panelResultados.SetActive(false);

        if (grupoIngresos != null) grupoIngresos.SetActive(false);
        if (grupoImpuestos != null) grupoImpuestos.SetActive(false);
        if (grupoBalance != null) grupoBalance.SetActive(false);

        if (textoIngresos != null)
        {
            textoIngresos.text = string.Empty;
            textoIngresos.rectTransform.localScale = escalaBaseIngresos;
        }

        if (textoImpuestos != null)
        {
            textoImpuestos.text = string.Empty;
            textoImpuestos.rectTransform.localScale = escalaBaseImpuestos;
        }

        if (textoBalance != null)
        {
            textoBalance.text = string.Empty;
            textoBalance.rectTransform.localScale = escalaBaseBalance;
        }

        Debug.Log("[EndscreenResults] 🔄 Día reiniciado: pantalla de resultados oculta y escalas restauradas.");
    }

    private IEnumerator MostrarSecuencia()
    {
        if (panelResultados == null)
            yield break;

        panelResultados.SetActive(true);
        skipSolicitado = false;

        if (grupoIngresos != null) grupoIngresos.SetActive(false);
        if (grupoImpuestos != null) grupoImpuestos.SetActive(false);
        if (grupoBalance != null) grupoBalance.SetActive(false);

        yield return new WaitForSeconds(delayInicio);

        goalsManager.CalcularResultadosDia(dayLogic.currentDay);
        float ingresos = goalsManager.GetIngresosFinales();
        float impuestos = goalsManager.GetImpuestosFinales();
        float balance = goalsManager.GetBalanceFinal();

        estadoActual = EstadoPantalla.MostrandoGrupos;
        yield return StartCoroutine(AparecerGrupos(impuestos));

        estadoActual = EstadoPantalla.MostrandoIngresos;
        yield return StartCoroutine(AnimarNumeroConEscala(textoIngresos, ingresos));

        estadoActual = EstadoPantalla.MostrandoBalance;
        yield return StartCoroutine(AnimarBalanceAleatorio(textoBalance, balance));

        estadoActual = EstadoPantalla.Finalizada;
        yield return new WaitForSeconds(delayAntesDeCalcular);

        goalsManager.IniciarCierreFinal();
        Debug.Log("[EndscreenResults] 🎬 Secuencia completada.");
    }

    private IEnumerator AparecerGrupos(float impuestos)
    {
        skipSolicitado = false;

        if (grupoIngresos != null)
        {
            grupoIngresos.SetActive(true);
            if (textoIngresos != null) textoIngresos.text = "¥0";
            yield return EsperarConSkip(tiempoEntreGrupos);
        }

        if (skipSolicitado)
        {
            MostrarGruposInstantaneos(impuestos);
            yield break;
        }

        if (grupoImpuestos != null)
        {
            grupoImpuestos.SetActive(true);
            if (textoImpuestos != null) textoImpuestos.text = $"¥{impuestos:N0}";
            yield return EsperarConSkip(tiempoEntreGrupos);
        }

        if (skipSolicitado)
        {
            MostrarGruposInstantaneos(impuestos);
            yield break;
        }

        if (grupoBalance != null)
        {
            grupoBalance.SetActive(true);
            if (textoBalance != null) textoBalance.text = "¥0";
        }

        yield return EsperarConSkip(tiempoEntreAnimaciones);
    }

    private void MostrarGruposInstantaneos(float impuestos)
    {
        if (grupoIngresos != null) grupoIngresos.SetActive(true);
        if (grupoImpuestos != null) grupoImpuestos.SetActive(true);
        if (grupoBalance != null) grupoBalance.SetActive(true);

        if (textoIngresos != null) textoIngresos.text = "¥0";
        if (textoImpuestos != null) textoImpuestos.text = $"¥{impuestos:N0}";
        if (textoBalance != null) textoBalance.text = "¥0";
    }

    //───────────────────────────────────────────────
    //  Animación de ingresos con rebote final
    //───────────────────────────────────────────────
    private IEnumerator AnimarNumeroConEscala(TextMeshProUGUI texto, float objetivo)
    {
        float duracionCalculada = Mathf.Max(2f, (objetivo / 1000f) * segundosPorMil);
        float duracionFinal = Mathf.Min(duracionCalculada, duracionMaximaIngresos);
        skipSolicitado = false;

        float tiempo = 0f;
        Vector3 escalaBase = escalaBaseIngresos;
        float escalaFinalExtra = escalaExtra * porcentajeEscalaFinalIngresos;
        Vector3 escalaFinal = escalaBase * (1f + escalaFinalExtra);

        // Fase de conteo + escala ascendente
        while (tiempo < duracionFinal && !skipSolicitado)
        {
            tiempo += Time.deltaTime;
            float progreso = Mathf.Clamp01(tiempo / duracionFinal);
            float valor = Mathf.Lerp(0f, objetivo, progreso);
            texto.text = $"¥{Mathf.RoundToInt(valor):N0}";
            texto.rectTransform.localScale = escalaBase * (1f + Mathf.Sin(progreso * Mathf.PI * 0.5f) * escalaExtra);
            yield return null;
        }

        // Rebote final (encoge → escala final)
        float t = 0f;
        Vector3 escalaMenor = escalaFinal * reboteEscalaFactor;
        while (t < 1f)
        {
            t += Time.deltaTime / reboteDuracion;
            float curva = Mathf.SmoothStep(0f, 1f, t);
            texto.rectTransform.localScale = Vector3.Lerp(escalaMenor, escalaFinal, curva);
            yield return null;
        }

        texto.text = $"¥{objetivo:N0}";
        texto.rectTransform.localScale = escalaFinal;

        yield return EsperarConSkip(tiempoEntreAnimaciones);
    }

    private IEnumerator AnimarBalanceAleatorio(TextMeshProUGUI texto, float balanceReal)
    {
        skipSolicitado = false;
        float tiempo = 0f;
        float intervalo = 0.05f;
        Vector3 escalaBase = escalaBaseBalance;
        float escalaObjetivo = 1f + escalaBalanceDuranteAnimacion;

        while (tiempo < duracionAleatoriaBalance && !skipSolicitado)
        {
            tiempo += intervalo;
            float t = Mathf.Clamp01(tiempo / duracionAleatoriaBalance);
            texto.rectTransform.localScale = escalaBase * Mathf.Lerp(1f, escalaObjetivo, t);
            float valor = balanceReal + Random.Range(-rangoBalanceAleatorio, rangoBalanceAleatorio);
            texto.text = $"¥{Mathf.RoundToInt(valor):N0}";
            yield return new WaitForSeconds(intervalo);
        }

        float tRegreso = 0f;
        while (tRegreso < 1f)
        {
            tRegreso += Time.deltaTime / duracionRegresoBalance;
            float escalaActual = Mathf.Lerp(escalaObjetivo, 1f, tRegreso);
            texto.rectTransform.localScale = escalaBase * escalaActual;
            yield return null;
        }

        texto.rectTransform.localScale = escalaBase;
        texto.text = $"¥{balanceReal:N0}";
    }

    private IEnumerator EsperarConSkip(float segundos)
    {
        float tiempo = 0f;
        skipSolicitado = false;
        while (tiempo < segundos && !skipSolicitado)
        {
            tiempo += Time.deltaTime;
            yield return null;
        }
    }
}