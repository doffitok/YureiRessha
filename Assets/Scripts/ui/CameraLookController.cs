using UnityEngine;
using System.Collections;

////////////////////////////////////////////////////////////////////////////////////////////
// controla ese movimiento chikito de la camara
//
// rota la camara entre dos posiciones principales
// las rotaciones son suaves y usan interpolacion esferica
// mantiene una corutina activa para controlar la animacion sin interferencias
////////////////////////////////////////////////////////////////////////////////////////////

public class CameraLookController : MonoBehaviour
{
    ////////////////////////////////////////////////////////////////////////////////////////////
    // configuraciones de rotacion objetivo
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Rotaciones objetivo")]
    public Vector3 frontRotation = new Vector3(10f, 0f, 0f);
    public Vector3 creditsRotation = new Vector3(10f, 180f, 0f);
    public float rotationSpeed = 1f;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // variables internas de control
    ////////////////////////////////////////////////////////////////////////////////////////////
    private Coroutine rotationCoroutine;

    ////////////////////////////////////////////////////////////////////////////////////////////
    // rota la camara para mirar hacia los creditos
    ////////////////////////////////////////////////////////////////////////////////////////////
    public void LookAtCredits()
    {
        if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);
        rotationCoroutine = StartCoroutine(RotateCamera(creditsRotation));
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // rota la camara para mirar hacia el frente
    ////////////////////////////////////////////////////////////////////////////////////////////
    public void LookAtFront()
    {
        if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);
        rotationCoroutine = StartCoroutine(RotateCamera(frontRotation));
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    // corutina de rotacion suave de la camara
    ////////////////////////////////////////////////////////////////////////////////////////////
    private IEnumerator RotateCamera(Vector3 targetEuler)
    {
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.Euler(targetEuler);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * rotationSpeed;
            transform.rotation = Quaternion.Slerp(startRot, targetRot, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        transform.rotation = targetRot;
    }
}