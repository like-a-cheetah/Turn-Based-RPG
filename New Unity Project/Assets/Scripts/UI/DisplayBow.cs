using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplayBow : ItemButton
{
    public RectTransform frame;

    bool selected;

    //void Update()
    //{
    //    text.text = player.bowPoint.ToString();
    //    if (player.bowPoint <= 0)
    //    {
    //        this.GetComponent<Button>().animator.SetTrigger("Normal");
    //        this.GetComponent<Image>().color = Color.black;
    //        selected = player.useBow = false;
    //    }
    //    else
    //        this.GetComponent<Image>().color = Color.white;

    //    if (player.useBow && selected)
    //    {
    //        selected = false;
    //        this.GetComponent<RectTransform>().sizeDelta = new Vector2(22.5f, 22.5f);
    //        frame.GetComponent<RectTransform>().sizeDelta = new Vector2(70f, 70f);
    //    }
    //    else
    //    {
    //        this.GetComponent<RectTransform>().sizeDelta = new Vector2(19, 19);
    //        frame.GetComponent<RectTransform>().sizeDelta = new Vector2(60f, 60f);
    //    }
    //}

    //public void ActiveItem()
    //{
    //    if (player.GetComponent<Player>().useSword)
    //        player.GetComponent<Player>().useSword = false;
        
    //    if (player.bowPoint > 0)
    //    {
    //        if(player.GetComponent<Player>().useBow == false)
    //            player.GetComponent<Player>().useBow = true;
    //        else
    //            player.GetComponent<Player>().useBow = false;

    //        if (player.GetComponent<Player>().useBow)
    //            selected = true;
    //    }
    //    return;
    //}
}