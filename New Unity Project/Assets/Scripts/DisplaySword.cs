using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplaySword : MonoBehaviour
{
    public Player player;
    public TextMeshProUGUI text;
    public RectTransform frame;

    bool selected;

    void Update()
    {
        text.text = player.swordPoint.ToString();

        if (player.swordPoint <= 0)
        {
            this.GetComponent<Button>().animator.SetTrigger("Normal");
            this.GetComponent<Image>().color = Color.black;
            player.useSword = selected = false;
        }
        else
            this.GetComponent<Image>().color = Color.white;

        if (player.useSword && selected)
        {
            selected = false;
            this.GetComponent<RectTransform>().sizeDelta = new Vector2(22.5f, 22.5f);
            frame.GetComponent<RectTransform>().sizeDelta = new Vector2(70f, 70f);
        }
        else
        {
            this.GetComponent<RectTransform>().sizeDelta = new Vector2(19, 19);
            frame.GetComponent<RectTransform>().sizeDelta = new Vector2(60f, 60f);
        }
    }

    public void ActiveItem()
    {
        if (player.GetComponent<Player>().useBow)
            player.GetComponent<Player>().useBow = false;

        if (player.swordPoint > 0)
        {
            player.GetComponent<Player>().useSword = !player.GetComponent<Player>().useSword;
            if (player.GetComponent<Player>().useSword)
                selected = true;
        }
        return;
    }
}
