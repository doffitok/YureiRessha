using UnityEngine;
using System.Collections;

public class CameraLookController : MonoBehaviour
{
    [Header("Rotaciones objetivo")]
    public Vector3 frontRotation = new Vector3(10f, 0f, 0f);
    public Vector3 creditsRotation = new Vector3(10f, 180f, 0f); // mira hacia atrás del vagón
    public float rotationSpeed = 1f;

    private bool isLookingAtCredits = false;
    private Coroutine rotationCoroutine;

    public void LookAtCredits()
    {
        if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);
        rotationCoroutine = StartCoroutine(RotateCamera(creditsRotation));
        isLookingAtCredits = true;
    }

    public void LookAtFront()
    {
        if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);
        rotationCoroutine = StartCoroutine(RotateCamera(frontRotation));
        isLookingAtCredits = false;
    }

    IEnumerator RotateCamera(Vector3 targetEuler)
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
