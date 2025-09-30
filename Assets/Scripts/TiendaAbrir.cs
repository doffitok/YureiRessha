using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]

/// <summary>
/// Control simple para abrir/cerrar la ventana "Tienda" hecha con UI Builder (UI Toolkit).
/// - Busca por ids/nombres en el UXML: "TiendaBoton", "TiendaAbierta" y "Exit"
/// - Muestra/oculta TiendaAbierta cambiando style.display (None / Flex)
/// - Provee métodos públicos OpenTienda/CloseTienda para llamar desde Fungus o desde otros scripts.
/// </summary>
public class TiendaUIController : MonoBehaviour
{
    [Header("Referencias (opcional)")]
    [Tooltip("Si el script no está en el mismo GameObject que el UIDocument, arrastra la referencia aquí.")]
    public UIDocument uiDocument;

    // IDs que buscaremos en el UXML. Cambia si tus nombres son distintos.
    [Header("IDs en UI Builder")]
    public string tiendaButtonId = "TiendaBoton";
    public string tiendaContainerId = "TiendaAbierta";
    public string exitButtonId = "Exit";

    // Referencias runtime
    private VisualElement root;
    private Button tiendaButton;
    private VisualElement tiendaContainer;
    private Button exitButton;

    private void OnEnable()
    {
        // Obtener UIDocument (auto-asignar si el componente está en el mismo GameObject)
        UIDocument doc = uiDocument != null ? uiDocument : GetComponent<UIDocument>();
        if (doc == null)
        {
            Debug.LogWarning("[TiendaUIController] No se encontró UIDocument. Asigna uno en el Inspector o añade este script al GameObject con UIDocument.");
            return;
        }

        root = doc.rootVisualElement;
        if (root == null)
        {
            Debug.LogWarning("[TiendaUIController] rootVisualElement es null.");
            return;
        }

        // Buscar elementos por ID (name que asignas en UI Builder)
        tiendaButton = root.Q<Button>(tiendaButtonId);
        tiendaContainer = root.Q<VisualElement>(tiendaContainerId);
        exitButton = root.Q<Button>(exitButtonId);

        // Ocultar por defecto la tienda si existe
        if (tiendaContainer != null)
        {
            tiendaContainer.style.display = DisplayStyle.None;
        }

        // Suscribir eventos
        if (tiendaButton != null)
            tiendaButton.clicked += OnTiendaButtonClicked;
        else
            Debug.LogWarning($"[TiendaUIController] No se encontró Button con id '{tiendaButtonId}'");

        if (exitButton != null)
            exitButton.clicked += OnExitButtonClicked;
        else
            Debug.LogWarning($"[TiendaUIController] No se encontró Button con id '{exitButtonId}' (si el Exit está dentro de TiendaAbierta, asegúrate del id)");
    }

    private void OnDisable()
    {
        if (tiendaButton != null)
            tiendaButton.clicked -= OnTiendaButtonClicked;
        if (exitButton != null)
            exitButton.clicked -= OnExitButtonClicked;
    }

    // Callback del botón Tienda
    private void OnTiendaButtonClicked()
    {
        // Si la tienda está oculta, abrir; si está abierta, cerrarla
        if (tiendaContainer == null) return;

        var current = tiendaContainer.style.display;
        if (current == DisplayStyle.None)
            OpenTienda();
        else
            CloseTienda();
    }

    // Callback del botón Exit
    private void OnExitButtonClicked()
    {
        CloseTienda();
    }

    // Métodos públicos para usar desde Fungus (Call Method) o desde otros scripts
    public void OpenTienda()
    {
        if (tiendaContainer == null)
        {
            Debug.LogWarning("[TiendaUIController] OpenTienda: tiendaContainer es null.");
            return;
        }

        tiendaContainer.style.display = DisplayStyle.Flex; // o DisplayStyle.Block según tu layout
        // opcional: llevar foco a un elemento dentro de la tienda, etc.
    }

    public void CloseTienda()
    {
        if (tiendaContainer == null) return;
        tiendaContainer.style.display = DisplayStyle.None;
    }

    // Útil: Toggle público
    public void ToggleTienda()
    {
        if (tiendaContainer == null) return;
        var current = tiendaContainer.style.display;
        if (current == DisplayStyle.None) OpenTienda(); else CloseTienda();
    }
}
