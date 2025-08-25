using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerStat : Stat
{
    public const int maxStamina = 20;
    public int stamina { get; protected set; }

    public Action<int> onStaminaChange;
    public Action onStaminaZero;

    protected override void Start()
    {
        base.Start();

        ChargingStamina(maxStamina);
    }

    public void InitManager(UIManager ui)
    {
        ui.SetMaxVal(maxHp, maxStamina);

        onStaminaChange += ui.UpdateStamina;
        onHpChange += ui.UpdateHp;
    }

    public void UseStamina()
    {
        if (stamina > 1)
        {
            stamina--;
            onStaminaChange(stamina);
        }
        else
        {
            SetHp(hp - 1);
        }
    }
        
    public void ChargingStamina(int charingVal)
    {
        stamina = Mathf.Clamp(stamina + charingVal, 0, maxStamina);
        onStaminaChange(stamina);
    }
}
