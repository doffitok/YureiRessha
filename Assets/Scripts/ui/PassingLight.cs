using UnityEngine;

public class PassingLight : MonoBehaviour
{
    public float speed = 10f;
    public float delay = 5f;
    public Vector3 startPos;
    public Vector3 endPos;

    private float timer;

    void Start()
    {
        transform.position = startPos;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= delay)
        {
            // Mover la luz
            transform.position = Vector3.Lerp(startPos, endPos, Mathf.PingPong((Time.time - delay) * speed, 1f));
        }
    }
}
