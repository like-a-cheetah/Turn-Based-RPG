using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyBar : MonoBehaviour
{
    public Player player;
    public Text text;

    private void Start()
    {
        GetComponent<Image>().fillAmount = (float)player.food / 10;
    }

    void Update()
    {
        if (player.food <= 9)
            text.color = Color.white;
        else
            text.color = Color.black;

        text.text = player.food.ToString();
        GetComponent<Image>().fillAmount = (float)player.food / 20;
    }
}
