using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    private GameObject player;

    void Awake()
    {
        player = GameObject.Find("Player");
        this.transform.position = player.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
