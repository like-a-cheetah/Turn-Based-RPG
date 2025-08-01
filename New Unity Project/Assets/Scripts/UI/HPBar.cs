using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    public Player player;
    public Text text;

    private void Start()
    {
        GetComponent<Image>().fillAmount = (float)player.HP / 10;
    }

    void Update()
    {
        if (player.HP <= 4)
            text.color = Color.white;
        else
            text.color = Color.black;

        text.text = player.HP.ToString();
        GetComponent<Image>().fillAmount = (float)player.HP / 10;
    }
}
