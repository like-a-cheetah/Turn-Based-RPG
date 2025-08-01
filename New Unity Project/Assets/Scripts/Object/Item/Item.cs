using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public GameObject parent;

    [SerializeField]
    private EItem itemType;
    [SerializeField]
    private float chargeVal;

    void Start()
    {
        this.transform.position = parent.transform.position;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            player.ItemCharging(itemType, chargeVal);
        }
    }
}
