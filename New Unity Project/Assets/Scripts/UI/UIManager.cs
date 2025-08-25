using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Canvas UI;
    
    private Slider hp;
    private Slider stamina;

    private List<ItemButton> btns = new List<ItemButton>();

    private Dictionary<Item.EItem, ItemButton> buttonMap = new Dictionary<Item.EItem, ItemButton>();

    private void Awake()
    {
        UI = Instantiate(UI);

        List<Slider> sliders = new List<Slider>();
        UI.GetComponentsInChildren<Slider>(sliders);

        foreach (var s in sliders)
        {
            if (s.name.Contains("HP"))
                hp = s;
            else if (s.name.Contains("Stamina"))
                stamina = s;
        }
        
        UI.GetComponentsInChildren<ItemButton>(btns);
        foreach(var btn in btns)
        {
            buttonMap[btn.GetItemType()] = btn;
        }
    }

    private void Start()
    {
    }

    public void UpdateHp(int val)
    {
        hp.value = val;
    }

    public void UpdateStamina(int val)
    {
        stamina.value = val;
    }

    public void SetMaxVal(int maxHp, int maxStamina)
    {
        hp.maxValue = maxHp;
        stamina.maxValue = maxStamina;
    }

    public void TakeItem(Item newItem)
    {
        buttonMap[newItem.itemType]?.ItemAdd(newItem.n);
    }
}
