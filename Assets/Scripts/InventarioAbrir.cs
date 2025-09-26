using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    // Inventario
    private VisualElement inventarioBox;
    private Button btnInventario;
    private Button btnCerrarInventario;

    // Tienda
    private VisualElement tiendaAbierta;
    private Button btnTiendaBoton;
    private Button btnExit;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // =================
        // Inventario
        // =================
        inventarioBox = root.Q<VisualElement>("InventarioBox");
        btnInventario = root.Q<Button>("Inventario");
        btnCerrarInventario = root.Q<Button>("CerrarInventario");

        if (inventarioBox != null)
            inventarioBox.style.display = DisplayStyle.None;

        if (btnInventario != null)
            btnInventario.clicked += () =>
            {
                if (inventarioBox != null)
                    inventarioBox.style.display = DisplayStyle.Flex;
            };

        if (btnCerrarInventario != null)
            btnCerrarInventario.clicked += () =>
            {
                if (inventarioBox != null)
                    inventarioBox.style.display = DisplayStyle.None;
            };

        // =================
        // Tienda
        // =================
        tiendaAbierta = root.Q<VisualElement>("TiendaAbierta");
        btnTiendaBoton = root.Q<Button>("TiendaBoton");
        btnExit = root.Q<Button>("Exit");

        if (tiendaAbierta != null)
            tiendaAbierta.style.display = DisplayStyle.None;

        if (btnTiendaBoton != null)
            btnTiendaBoton.clicked += () =>
            {
                if (tiendaAbierta != null)
                    tiendaAbierta.style.display = DisplayStyle.Flex;
            };

        if (btnExit != null)
            btnExit.clicked += () =>
            {
                if (tiendaAbierta != null)
                    tiendaAbierta.style.display = DisplayStyle.None;
            };
    }
}
