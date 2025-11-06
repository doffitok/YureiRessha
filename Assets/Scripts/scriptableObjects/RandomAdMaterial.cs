using UnityEngine;

public class RandomAdMaterial : MonoBehaviour
{
    [Header("Planos donde van los anuncios")]
    public Renderer[] adPlanes; // arrastra aquí los 2 planos del vagón

    [Header("Materiales posibles (anuncios)")]
    public Material[] adMaterials; // arrastra aquí tus materiales de anuncios

    [Header("Configuración")]
    public bool sameAdForBoth = true; // si true, ambos planos mostrarán el mismo anuncio

    void Start()
    {
        if (adPlanes.Length == 0 || adMaterials.Length == 0)
        {
            Debug.LogWarning("Faltan planos o materiales en RandomAdMaterial");
            return;
        }

        // 🔹 Escoge un material aleatorio
        int randomIndex1 = Random.Range(0, adMaterials.Length);

        if (sameAdForBoth)
        {
            // 🔹 Ambos planos muestran el mismo ad
            foreach (Renderer plane in adPlanes)
            {
                plane.material = adMaterials[randomIndex1];
            }
        }
        else
        {
            // 🔹 Cada plano tiene un ad distinto
            foreach (Renderer plane in adPlanes)
            {
                int randomIndex = Random.Range(0, adMaterials.Length);
                plane.material = adMaterials[randomIndex];
            }
        }
    }
}
