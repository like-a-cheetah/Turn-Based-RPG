using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public GameObject parent;
    void Start()
    {
        this.transform.position = parent.transform.position;
    }
}
