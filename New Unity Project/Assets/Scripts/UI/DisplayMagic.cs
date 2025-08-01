using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplayMagic : ItemButton
{
    public GameObject bomb;
    public GameObject magicBomb;

    public void UseItem()
    {
        //if (player.magicPoint > 0 && GameManager.instance.playersTurn)
        //{
        //    bombSound.Play();
        //    var bombInstance = Instantiate<GameObject>(this.magicBomb);
        //    player.magicPoint--;

        //    StartCoroutine(bombInstance.GetComponent<Boom>().Remove());
        //    StartCoroutine(player.MagicAttack());
        //}
    }
}
