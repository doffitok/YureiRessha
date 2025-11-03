using UnityEngine;
using UnityEngine.UI;

public class PortraitFloat : MonoBehaviour
{
    public float amplitude = 10f;     // Qué tanto se mueve
    public float speed = 1f;          // Qué tan rápido se mueve
    public bool randomOffset = true;  // Si cada retrato tiene movimiento distinto

    private RectTransform rect;
    private Vector2 startPos;
    private float offset;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;
        offset = randomOffset ? Random.Range(0f, 100f) : 0f;
    }

    void Update()
    {
        float x = Mathf.Sin((Time.time + offset) * speed) * amplitude * 0.2f; // movimiento leve en X
        float y = Mathf.Sin((Time.time + offset) * speed * 1.3f) * amplitude; // movimiento leve en Y

        rect.anchoredPosition = startPos + new Vector2(x, y);
    }
}
