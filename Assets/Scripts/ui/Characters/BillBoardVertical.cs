using UnityEngine;

public class BillboardFixedX : MonoBehaviour
{
    public Camera targetCamera;
    [Range(0f, 1f)] public float smoothSpeed = 0.1f;
    public float fixedXRotation = 80f; // inclinación fija en X

    void LateUpdate()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
            return;

        // Dirección desde el objeto hacia la cámara, solo en el plano horizontal
        Vector3 direction = targetCamera.transform.position - transform.position;
        direction.y = 0f; // evita que se incline hacia arriba o abajo

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);

            // Aplicamos la rotación con X fijo
            Vector3 euler = lookRotation.eulerAngles;
            euler.x = fixedXRotation; // bloqueamos X
            euler.z = 0f; // aseguramos que no se incline lateralmente

            Quaternion finalRotation = Quaternion.Euler(euler);
            transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, smoothSpeed);
        }
    }
}
