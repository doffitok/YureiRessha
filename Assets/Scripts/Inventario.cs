using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventario : MonoBehaviour
{
    public Dictionary<string, int> inventory = new Dictionary<string, int>();
    public static Inventario Instance;
    public GameObject slotItem;
    public Transform slotContainer;

    private void Awake()
    {
        Instance = this;
    }

    public void AddItem(string nameItem, int amount)
    {
        if (inventory.ContainsKey(nameItem))
        {
            inventory[nameItem] += amount;
        }
        else
        {
            inventory.Add(nameItem, amount);
        }

    }
    public void SubstractItem(string nameItem, int amount)
    {
        if (inventory.ContainsKey(nameItem))
        {
            if (inventory[nameItem] >= amount)
            {
                inventory[nameItem] -= amount;
            }
        }
    }
}
