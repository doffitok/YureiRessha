using UnityEngine;

[RequireComponent(typeof(Light))]
public class RandomFlickerLight : MonoBehaviour
{
    public float minIntensity = 0.8f;   // brillo mínimo
    public float maxIntensity = 1.2f;   // brillo máximo
    public float flickerSpeed = 0.1f;   // velocidad de parpadeo base
    public float randomChance = 0.05f;  // probabilidad de un flicker más notorio

    private Light lightSource;
    private float targetIntensity;

    void Start()
    {
        lightSource = GetComponent<Light>();
        targetIntensity = lightSource.intensity;
        StartCoroutine(FlickerRoutine());
    }

    System.Collections.IEnumerator FlickerRoutine()
    {
        while (true)
        {
            // Pequeño parpadeo orgánico
            float noise = Mathf.PerlinNoise(Time.time * 10f, transform.position.x * 2f);
            lightSource.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);

            // Ocasional “glitch” aleatorio
            if (Random.value < randomChance)
            {
                lightSource.intensity = Random.Range(minIntensity * 0.5f, maxIntensity * 1.5f);
                yield return new WaitForSeconds(Random.Range(0.02f, 0.08f));
            }

            yield return new WaitForSeconds(flickerSpeed);
        }
    }
}
