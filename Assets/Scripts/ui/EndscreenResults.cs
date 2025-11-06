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

    [Header("Animación de Balance")]
    [SerializeField] private float duracionAleatoriaBalance = 3f;
    [SerializeField] private float rangoBalanceAleatorio = 3000f;
    [SerializeField] private float escalaBalanceDuranteAnimacion = 0.1f;
    [SerializeField] private float duracionRegresoBalance = 0.4f;

    private enum EstadoPantalla { Inactiva, MostrandoGrupos, MostrandoIngresos, MostrandoBalance, Finalizada }
    private EstadoPantalla estadoActual = EstadoPantalla.Inactiva;
    private bool skipSolicitado = false;

    // Guardamos la escala original de cada texto
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

        // Guardar escalas iniciales
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
        // ✅ Compatible con el nuevo Input System
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
            textoIngresos.rectTransform.localScale = escalaBaseIngresos; // 🔁 restaurar escala
        }

        if (textoImpuestos != null)
        {
            textoImpuestos.text = string.Empty;
            textoImpuestos.rectTransform.localScale = escalaBaseImpuestos; // 🔁 restaurar escala
        }

        if (textoBalance != null)
        {
            textoBalance.text = string.Empty;
            textoBalance.rectTransform.localScale = escalaBaseBalance; // 🔁 restaurar escala
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

        //───────────────────────────────────────────────
        // 1️⃣ Calcular resultados reales
        //───────────────────────────────────────────────
        goalsManager.CalcularResultadosDia(dayLogic.currentDay);
        float ingresos = goalsManager.GetIngresosFinales();
        float impuestos = goalsManager.GetImpuestosFinales();
        float balance = goalsManager.GetBalanceFinal();

        //───────────────────────────────────────────────
        // 2️⃣ Mostrar grupos uno por uno (clic = mostrar todos)
        //───────────────────────────────────────────────
        estadoActual = EstadoPantalla.MostrandoGrupos;
        yield return StartCoroutine(AparecerGrupos(impuestos));

        //───────────────────────────────────────────────
        // 3️⃣ Animar ingresos (clic = saltar animación)
        //───────────────────────────────────────────────
        estadoActual = EstadoPantalla.MostrandoIngresos;
        yield return StartCoroutine(AnimarNumeroConEscala(textoIngresos, ingresos));

        //───────────────────────────────────────────────
        // 4️⃣ Animar balance (clic = saltar animación)
        //───────────────────────────────────────────────
        estadoActual = EstadoPantalla.MostrandoBalance;
        yield return StartCoroutine(AnimarBalanceAleatorio(textoBalance, balance));

        //───────────────────────────────────────────────
        // 5️⃣ Fin
        //───────────────────────────────────────────────
        estadoActual = EstadoPantalla.Finalizada;
        yield return new WaitForSeconds(delayAntesDeCalcular);

        goalsManager.IniciarCierreFinal();
        Debug.Log("[EndscreenResults] 🎬 Secuencia completada.");
    }

    //───────────────────────────────────────────────
    //  Aparecer los grupos (con skip)
    //───────────────────────────────────────────────
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
    //  Animación de ingresos (conteo + escala + skip)
    //───────────────────────────────────────────────
    private IEnumerator AnimarNumeroConEscala(TextMeshProUGUI texto, float objetivo)
    {
        float duracionCalculada = Mathf.Max(2f, (objetivo / 1000f) * segundosPorMil);
        float duracionFinal = Mathf.Min(duracionCalculada, duracionMaximaIngresos);
        skipSolicitado = false;

        float tiempo = 0f;
        Vector3 escalaBase = escalaBaseIngresos;
        float escalaFinalExtra = escalaExtra * porcentajeEscalaFinalIngresos;

        while (tiempo < duracionFinal && !skipSolicitado)
        {
            tiempo += Time.deltaTime;
            float progreso = Mathf.Clamp01(tiempo / duracionFinal);
            float valor = Mathf.Lerp(0f, objetivo, progreso);
            texto.text = $"¥{Mathf.RoundToInt(valor):N0}";
            texto.rectTransform.localScale = escalaBase * (1f + Mathf.Sin(progreso * Mathf.PI * 0.5f) * escalaExtra);
            yield return null;
        }

        texto.rectTransform.localScale = escalaBase * (1f + escalaFinalExtra);
        texto.text = $"¥{objetivo:N0}";
        yield return EsperarConSkip(tiempoEntreAnimaciones);
    }

    //───────────────────────────────────────────────
    //  Animación de balance (aleatorio + escala + skip)
    //───────────────────────────────────────────────
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

        // Vuelta suave a la escala original
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

    //───────────────────────────────────────────────
    //  Helper para pausas que se pueden adelantar
    //───────────────────────────────────────────────
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