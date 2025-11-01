using UnityEngine;

public class CameraIdleLoop : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 0.5f;     // Velocidad general del loop
    public float loopDistance = 2f;    // Distancia de ida y vuelta
    public float smoothness = 0.5f;    // Qué tan suave es el ease-in/out (0.1 = brusco, 1 = muy suave)

    [Header("Tilt")]
    public float tiltAmount = 2f;      // Grados máximos de inclinación
    public float tiltSpeed = 1f;       // Velocidad del tilt

    private Vector3 startPos;
    private float timeCounter;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        timeCounter += Time.deltaTime * moveSpeed;

        // Movimiento con suavizado (usa una curva senoidal que tiene easy-in/easy-out natural)
        float t = Mathf.PingPong(timeCounter, 1f);
        float easedT = Mathf.SmoothStep(0f, 1f, t); // ← aquí se suaviza
        float zOffset = Mathf.Lerp(0f, loopDistance, easedT);

        transform.position = startPos + transform.forward * zOffset;

        // Tilt orgánico: mezcla senoidal y un pequeño ruido aleatorio para que se sienta “vivo”
        float tiltX = Mathf.Sin(Time.time * tiltSpeed * 0.7f) * tiltAmount * 0.5f;
        float tiltZ = Mathf.Sin(Time.time * tiltSpeed) * tiltAmount;
        transform.localRotation = Quaternion.Euler(tiltX, 0f, tiltZ);
    }
}
