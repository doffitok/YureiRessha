using UnityEngine;

public class CameraIdleLoop : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 0.5f;     // Velocidad del loop
    public float loopDistance = 2f;    // Distancia de ida y vuelta
    public float smoothness = 0.5f;    // Suavidad del easing

    [Header("Tilt")]
    public float tiltAmount = 2f;      // Máximo tilt
    public float tiltSpeed = 1f;       // Velocidad del tilt

    [Header("Look Control")]
    public bool lookBackwards = false; // Activar para mirar al otro lado
    public Vector3 lookAxis = Vector3.up; // Eje alrededor del cual girará 180°

    private Vector3 startPos;
    private float timeCounter;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // --- Movimiento recto hacia adelante (PingPong)
        timeCounter += Time.deltaTime * moveSpeed;
        float t = Mathf.PingPong(timeCounter, 1f);
        float easedT = Mathf.SmoothStep(0f, 1f, t);
        float zOffset = Mathf.Lerp(0f, loopDistance, easedT);
        transform.position = startPos + transform.forward * zOffset;

        // --- Tilt orgánico
        float tiltX = Mathf.Sin(Time.time * tiltSpeed * 0.7f) * tiltAmount * 0.5f;
        float tiltZ = Mathf.Sin(Time.time * tiltSpeed) * tiltAmount;

        // --- Rotación base (sin tilt)
        Quaternion baseRotation = Quaternion.identity;

        // Si queremos mirar al otro lado, giramos 180° sobre el eje fijo (Vector3.up)
        if (lookBackwards)
            baseRotation = Quaternion.AngleAxis(180f, lookAxis);

        // --- Combinamos tilt con rotación base
        Quaternion tiltRotation = Quaternion.Euler(tiltX, 0f, tiltZ);
        transform.rotation = baseRotation * tiltRotation;
    }
}
