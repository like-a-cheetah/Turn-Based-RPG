using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplayFood : ItemButton
{
    public GameManager gameManager;
    public AudioSource eatingSound;

    //void Update()
    //{
    //    text.text = player.foodPoint.ToString();

    //    if (player.foodPoint <= 0)
    //    {
    //        this.GetComponent<Button>().animator.SetTrigger("Normal");
    //        this.GetComponent<Image>().color = Color.black;
    //    }
    //    else
    //        this.GetComponent<Image>().color = Color.white;
    //}

    //public void UseItem()
    //{
    //    if (player.foodPoint > 0 && GameManager.instance.playersTurn)
    //    {
    //        eatingSound.Play();
    //        player.foodPoint--;

    //        player.food += 10;
    //        if (player.food > 20)
    //            player.food = 20;

    //       // StartCoroutine(gameManager.MoveEnemies());
    //    }
    //}
}
