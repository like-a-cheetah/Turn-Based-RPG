using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplaySword : ItemButton
{
    public RectTransform frame;

    bool selected;

    protected override void Update()
    {
        base.Update();

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

    }
}
