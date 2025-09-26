using UnityEngine;
using UnityEngine.UIElements;

public class HUD : MonoBehaviour
{
    // Referencia al contador
    public DayLogic dayLogic;

    // Label del HUD
    private Label timeLabel;

    private void OnEnable()
    {
        // Obtener el root del UI Document
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Label del tiempo
        timeLabel = root.Q<Label>("timeLabel");

        // Buscar todos los botones dentro del HUD
        var buttons = root.Query<Button>().ToList();

        foreach (var btn in buttons)
        {
            // Hover: agrandar al pasar el mouse
            btn.RegisterCallback<MouseEnterEvent>(evt =>
            {
                btn.transform.scale = new Vector3(1.2f, 1.2f, 1f);
            });
            btn.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                btn.transform.scale = Vector3.one;
            });
        }

        // Botones
        Button startBtn = root.Q<Button>("EmpezarTerminar");
        Button tiendaBtn = root.Q<Button>("TiendaBoton");

        VisualElement tiendaPanel = root.Q<VisualElement>("Tienda");

        if (startBtn != null)
        {
            startBtn.clicked += () =>
            {
                if (dayLogic != null)
                    dayLogic.StartDay();          // Inicia el contador

                // Ocultar ambos botones
                startBtn.style.display = DisplayStyle.None;
                if (tiendaBtn != null)
                    tiendaBtn.style.display = DisplayStyle.None;

                if (tiendaPanel != null)
                        tiendaPanel.style.display = DisplayStyle.None;
                
               
            };
        }

        // TiendaBoton no hace nada al clicarse (por ahora)
    }

    private void Update()
    {
        // Actualizar el Label con el tiempo
        if (dayLogic != null && timeLabel != null)
        {
            timeLabel.text = "Tiempo transcurrido: " + dayLogic.currentSecond.ToString();
        }
    }
}
