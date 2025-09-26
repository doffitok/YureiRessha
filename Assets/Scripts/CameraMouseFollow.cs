using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMouseFollow : MonoBehaviour
{
    [Header("Configuración")]
    public float maxOffset = 0.1f;       // Máximo desplazamiento
    public float smoothSpeed = 0.5f;   // Velocidad de smoothing del movimiento

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        // Posición del mouse con Input System
        Vector2 mousePos = Mouse.current.position.ReadValue();

        float mouseX = -(mousePos.x / Screen.width - 0.5f) * 0.2f; // Movimiento vertical (Negativo para invertir el movimiento)
        float mouseY = (mousePos.y / Screen.height - 0.5f) * 0.2f; // Movimiento horizontal

        // Calculamos la posición objetivo
        Vector3 targetPosition = new Vector3(
            initialPosition.x + mouseX * maxOffset,
            initialPosition.y + mouseY * maxOffset,
            initialPosition.z
        );

        // Interpolación suave
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
    }
}