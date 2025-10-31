using UnityEngine;

public class CameraIdleLoop : MonoBehaviour
{
    [Header("Movimiento")]
    public float loopDistance = 5f;          // Distancia máxima hacia adelante
    public float loopDuration = 5f;          // Tiempo total ida+vuelta

    [Header("Tilt Orgánico")]
    public float tiltX = 1f;                 // Inclinación X
    public float tiltY = 0.5f;               // Inclinación Y
    public float tiltZ = 2f;                 // Inclinación Z
    public float tiltSpeed = 0.5f;           // Velocidad del balanceo

    private Vector3 startPos;
    private Vector3 endPos;

    void Start()
    {
        startPos = transform.position;
        endPos = startPos + transform.forward * loopDistance;
    }

    void Update()
    {
        // Movimiento ida-vuelta continuo sin pausas
        float t = Mathf.PingPong(Time.time / (loopDuration / 2f), 1f);
        transform.position = Vector3.Lerp(startPos, endPos, t);

        // Tilt orgánico con senos combinados
        float tiltEulerX = Mathf.Sin(Time.time * tiltSpeed) * tiltX;
        float tiltEulerY = Mathf.Sin(Time.time * tiltSpeed * 0.7f) * tiltY;
        float tiltEulerZ = Mathf.Sin(Time.time * tiltSpeed * 1.3f) * tiltZ;

        transform.localRotation = Quaternion.Euler(tiltEulerX, tiltEulerY, tiltEulerZ);
    }
}
