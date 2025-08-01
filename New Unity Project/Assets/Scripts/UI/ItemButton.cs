using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemButton : MonoBehaviour
{
    protected Player player;
    protected TextMeshProUGUI text;
    public AudioSource itemUseSound;

    [SerializeField]
    EItem itemType;

    float itemN;
    
    void Start()
    {
        
    }
    
    protected virtual void Update()
    {
        itemN = player.inven[itemType];

        text.text = itemN.ToString();

        if (itemN <= 0)
        {
            this.GetComponent<Button>().animator.SetTrigger("Normal");
            this.GetComponent<Image>().color = Color.black;
        }
        else
            this.GetComponent<Image>().color = Color.white;
    }

    protected virtual void UseItem()
    {
        player.UseItem(itemType);
    }
}
