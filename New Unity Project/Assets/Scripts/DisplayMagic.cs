using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplayMagic : MonoBehaviour
{
    public GameObject bomb;
    public Player player;
    public TextMeshProUGUI text;
    public GameObject magicBomb;
    public AudioSource bombSound;

    void Update()
    {
        text.text = player.magicPoint.ToString();

        if (player.magicPoint <= 0)
        {
            this.GetComponent<Button>().animator.SetTrigger("Normal");
            this.GetComponent<Image>().color = Color.black;
        }
        else
            this.GetComponent<Image>().color = Color.white;
    }

    public void UseItem()
    {
        if (player.magicPoint > 0 && GameManager.instance.playersTurn)
        {
            bombSound.Play();
            var bombInstance = Instantiate<GameObject>(this.magicBomb);
            player.magicPoint--;

            StartCoroutine(bombInstance.GetComponent<Boom>().Remove());
            StartCoroutine(player.MagicAttack());
        }
    }
}
