using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    public float chargeVal;
    
    [SerializeField]
    public static List<Item> items = new List<Item>();

    public static int weightsSum;

    [SerializeField]
    public int weight;

    public static LayerMask layer;

    public static void Init()
    {
        Item[] prefabs = Resources.LoadAll<Item>("DroppedItems");

        foreach (Item prefab in prefabs)
        {
            items.Add(prefab);
            weightsSum += prefab.weight;
        }
    }

    private void Awake()
    {
         layer = LayerMask.GetMask("Item");
    }

    void Start()
    {
    }

    public static Item CreateRandomItem()
    {
        int rand = UnityEngine.Random.Range(0, Item.weightsSum);

        int count = 0;

        for (int i = 0; i < items.Count; i++)
        {
            count += items[i].weight;
            if (rand < count)
            {
                return items[i];
            }
        }

        return null;
    }

    public virtual void Use()
    {
        
    }
}
