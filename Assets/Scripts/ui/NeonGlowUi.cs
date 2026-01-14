using UnityEngine;
using UnityEngine.UI;

public class NeonGlowUI : MonoBehaviour
{
    public Image glowImage;
    public float frequency = 2f;
    public float amplitude = 0.2f; // cambio de opacidad

    private float baseAlpha;

    void Start()
    {
        baseAlpha = glowImage.color.a;
    }

    void Update()
    {
        Color c = glowImage.color;
        c.a = baseAlpha + Mathf.Sin(Time.time * frequency) * amplitude;
        glowImage.color = c;
    }
}
