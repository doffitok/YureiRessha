using UnityEngine;
using UnityEngine.UI;

public class DebugAmistad : MonoBehaviour
{
    [Header("Referencias")]
    public PassengerData passengerData;
    public Button botonSubirAmistad;
    public Button botonBajarAmistad;
    public Text textoEstado;

    void Start()
    {
        // Configurar botones
        botonSubirAmistad.onClick.AddListener(SubirAmistad);
        botonBajarAmistad.onClick.AddListener(BajarAmistad);

        ActualizarUI();
    }

    public void SubirAmistad()
    {
        passengerData.CambiarAmistad(1);
        ActualizarUI();
        Debug.Log(passengerData.nombre + ": Amistad +1 = " + passengerData.amistad + " (" + passengerData.EstadoActual + ")");
    }

    public void BajarAmistad()
    {
        passengerData.CambiarAmistad(-1);
        ActualizarUI();
        Debug.Log(passengerData.nombre + ": Amistad -1 = " + passengerData.amistad + " (" + passengerData.EstadoActual + ")");
    }

    void ActualizarUI()
    {
        if (textoEstado != null)
        {
            textoEstado.text =
                passengerData.nombre +
                "\nAmistad: " + passengerData.amistad +
                "\nEstado: " + passengerData.EstadoActual;
        }
    }
}