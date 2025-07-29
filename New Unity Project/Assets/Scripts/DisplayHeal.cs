using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplayHeal : MonoBehaviour
{
    public Player player;
    public TextMeshProUGUI text;
    public GameManager gameManager;
    public AudioSource healSound;

    void Update()
    {
        text.text = player.healPoint.ToString();

        if (player.healPoint <= 0)
        {
            this.GetComponent<Button>().animator.SetTrigger("Normal");
            this.GetComponent<Image>().color = Color.black;
        }
        else
            this.GetComponent<Image>().color = Color.white;
    }

    public void UseItem()
    {
        if (player.healPoint > 0 && GameManager.instance.playersTurn)
        {
            healSound.Play();
            player.healPoint--;

            player.HP += 7;
            if (player.healPoint > 10)
                player.healPoint = 10;

            StartCoroutine(gameManager.MoveEnemies());
        }
    }
}
