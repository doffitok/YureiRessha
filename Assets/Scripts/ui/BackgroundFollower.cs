using UnityEngine;

public class BackgroundFollower : MonoBehaviour
{
    public Transform cameraTransform;
    public Vector3 offset = new Vector3(0, 0, 20);

    void LateUpdate()
    {
        if (cameraTransform != null)
            transform.position = cameraTransform.position + cameraTransform.forward * offset.z;
    }
}
