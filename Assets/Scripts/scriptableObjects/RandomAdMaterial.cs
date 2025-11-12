using UnityEngine;

public class RandomAdMaterial : MonoBehaviour
{
    [Header("Planos donde van los anuncios")]
    public Renderer[] adPlanes;

    [Header("Materiales posibles (anuncios)")]
    public Material[] adMaterials;

    [Header("Configuración")]
    public bool sameAdForBoth = true;
    
    [Tooltip("Evita que se repita el último anuncio")]
    public bool avoidRepeat = true;
    private int lastIndex = -1;

    void Start()
    {
        GenerateRandomAds();
    }

    // 🔹 Método público para regenerar en tiempo real
    public void GenerateRandomAds()
    {
        if (adPlanes.Length == 0 || adMaterials.Length == 0)
        {
            Debug.LogWarning("Faltan planos o materiales en RandomAdMaterial");
            return;
        }

        int randomIndex1 = GetRandomIndex();
        Debug.Log($"Material asignado: {adMaterials[randomIndex1].name}");

        if (sameAdForBoth)
        {
            foreach (Renderer plane in adPlanes)
            {
                plane.material = adMaterials[randomIndex1];
            }
        }
        else
        {
            foreach (Renderer plane in adPlanes)
            {
                int randomIndex = GetRandomIndex();
                plane.material = adMaterials[randomIndex];
            }
        }
    }

    private int GetRandomIndex()
    {
        if (adMaterials.Length == 0) return 0;
        
        if (adMaterials.Length == 1) return 0;

        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, adMaterials.Length);
        } while (avoidRepeat && randomIndex == lastIndex && adMaterials.Length > 1);

        lastIndex = randomIndex;
        return randomIndex;
    }

    // 🔹 Para probar manualmente
    [ContextMenu("Probar Cambio Aleatorio")]
    private void TestRandomChange()
    {
        GenerateRandomAds();
    }
}