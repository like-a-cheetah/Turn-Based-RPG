using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ItemButton : MonoBehaviour
{
    private Item item;

    private Button btn;

    private Text text;
    private Image fill;
    private Image image;

    private void Awake()
    {
        btn = GetComponent<Button>();

        btn.onClick.AddListener(UseItem);

        text = GetComponentInChildren<Text>();

        item = GetComponent<Item>();
        
        GameObject fillObj = transform.Find("Fill").gameObject;
        if(fillObj)
        {
            fill = fillObj.GetComponent<Image>();
        }

        GameObject imageObj = transform.Find("Image").gameObject;
        if(imageObj)
        {
            image = imageObj.GetComponent<Image>();
        }
    }

    private void Start()
    {
        item.n = 0;

        SetNum(item.n);
    }

    public void ItemAdd(float itemN)
    {
        item.n += itemN;

        SetNum(item.n);
    }

    public Item.EItem GetItemType()
    {
        return item.itemType;
    }

    private void UseItem()
    {
        if (item.n >= 1)
        {
            float n = item.UseItem();
            
            SetNum(n);
        }
    }

    public void SetNum(float n)
    {
        fill.fillAmount = n;

        text.text = n.ToString();

        btn.interactable = n >= 1;
    }
}
