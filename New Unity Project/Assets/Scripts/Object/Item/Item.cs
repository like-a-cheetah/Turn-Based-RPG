using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum EItem
    {
        Heal, Food, Sword, Arrow, Magic
    }

    [SerializeField]
    public EItem itemType;

    [SerializeField]
    public static List<Item> allItems = new List<Item>();

    public static int weightsSum;

    [SerializeField]
    public int weight;

    public static LayerMask layer;

    //public float n { protected set; get; }
    [SerializeField]
    public float n;

    public static void Init()
    {
        Item[] prefabs = Resources.LoadAll<Item>("DroppedItems");

        foreach (Item prefab in prefabs)
        {
            allItems.Add(prefab);
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

        for (int i = 0; i < allItems.Count; i++)
        {
            count += allItems[i].weight;
            if (rand < count)
            {
                return allItems[i];
            }
        }

        return null;
    }

    public virtual float UseItem()
    {
        print(name);

        return --n;
    }
}
