using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInform : MonoBehaviour
{
    public Player player;
    public Text text;
    // Start is called before the first frame update
    void Start()
    {
        text.text = player.HP.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
