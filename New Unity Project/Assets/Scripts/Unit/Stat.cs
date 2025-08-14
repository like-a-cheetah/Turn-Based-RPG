using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Stat : MonoBehaviour
{
    public int maxHp { get; protected set; }
    public int hp { get; protected set; }
    public int ad { get; protected set; }

    public Action<int> onHpChange;
    public Action onHpZero;

    public void Init(int inMaxHp, int inAd)
    {
        maxHp = inMaxHp;
        ad = inAd;
    }

    private void Start()
    {
        hp = maxHp;
    }

    protected void SetHp(int newHp)
    {
        newHp = Mathf.Clamp(newHp, 0, maxHp);
        hp = newHp;

        onHpChange?.Invoke(newHp);

        if(hp == 0) onHpZero?.Invoke();
    }

    public bool TakeDamage(int inDamage)
    {
        int newHp = Mathf.Clamp(hp - inDamage, 0, hp);
        SetHp(newHp);

        return hp <= 0;
    }

    protected void Heal(int heal)
    {
        int newHp = Mathf.Clamp(hp + heal, 0, maxHp);
        SetHp(newHp);
    }

    protected void SetHpMax()
    {
        SetHp(maxHp);
    }
}
