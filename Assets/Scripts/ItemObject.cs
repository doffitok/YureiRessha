using UnityEngine;

public class ItemObject : MonoBehaviour
{
    public ItemInventario itemProfile;

    private void Start()
    {
        foreach (Transform t in transform)
        {
            Destroy(t.gameObject);
        }
       // Instantiate(itemProfile.itemGameObject, transform);
    }

    private void OnMouseDown()
    {
        Inventario.Instance.AddItem(itemProfile.id, 1);
        Destroy(gameObject);
    }
}
