using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AlmanaqueUI : MonoBehaviour
{
    [Header("Datos del Almanaque")]
    public List<AlmanaqueInfo> personajes; // Arrastra tus ScriptableObjects aquí

    [Header("Referencias UI")]
    public UIDocument uiDocument;          // UIDocument con tu UXML
    public string panelIzquierdoName = "PanelIzquierdo";
    public string panelDerechoName = "PanelDerecho";
    public string tituloLabelName = "TituloLabel";
    public string descripcionLabelName = "DescripcionLabel";
    public string imagenPersonajeName = "ImagenPersonaje";
    public string imagenExtraName = "ImagenExtra";

    [Header("Estilo de Botón")]
    public Color colorFondo = new Color(0.17f, 0.17f, 0.23f);
    public Color colorHover = new Color(0.26f, 0.26f, 0.40f);
    public Color colorTexto = Color.white;
    public float radioBorde = 8f;
    public Vector2 sizeBoton = new Vector2(50, 50);
    public float margenInferior = 4f;

    private ScrollView panelIzquierdo;
    private VisualElement panelDerecho;
    private Label tituloLabel;
    private Label descripcionLabel;
    private VisualElement imagenPersonaje;
    private VisualElement imagenExtra;

    private void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        // 🔹 Referencias a UI
        panelIzquierdo = root.Q<ScrollView>(panelIzquierdoName);
        panelDerecho = root.Q<VisualElement>(panelDerechoName);
        tituloLabel = root.Q<Label>(tituloLabelName);
        descripcionLabel = root.Q<Label>(descripcionLabelName);
        imagenPersonaje = root.Q<VisualElement>(imagenPersonajeName);
        imagenExtra = root.Q<VisualElement>(imagenExtraName);

        // 🔹 Crear botones dinámicamente
        foreach (var personaje in personajes)
        {
            var boton = new Button();
            boton.style.backgroundImage = new StyleBackground(personaje.spritePersonaje);
            boton.AddToClassList("button-personaje"); // Aplica el estilo USS
            boton.clicked += () => MostrarInfo(personaje);
              panelIzquierdo.Add(boton);


            // Tooltip opcional: título al pasar mouse
            boton.tooltip = personaje.titulo;

            // Evento click
            boton.clicked += () => MostrarInfo(personaje);

            // Hover visual simple
            boton.RegisterCallback<MouseEnterEvent>(evt => boton.style.backgroundColor = colorHover);
            boton.RegisterCallback<MouseLeaveEvent>(evt => boton.style.backgroundColor = colorFondo);

            panelIzquierdo.Add(boton);
        }

        // 🔹 Mostrar el primero por defecto
        if (personajes.Count > 0) MostrarInfo(personajes[0]);
    }

    private void MostrarInfo(AlmanaqueInfo info)
    {
        if (tituloLabel != null) tituloLabel.text = info.titulo;
        if (descripcionLabel != null) descripcionLabel.text = info.descripcion;
        if (imagenPersonaje != null) imagenPersonaje.style.backgroundImage = new StyleBackground(info.spritePersonaje);
        if (imagenExtra != null) imagenExtra.style.backgroundImage = new StyleBackground(info.spriteExtra);
    }
}
