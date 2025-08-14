using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine;
using System;

public class Player : MovingObject
{
    public GameManager GameManager;
    public GameObject arrow;
    public GameObject sideArrow;

    public int horizontal;
    public int vertical;

    private bool turn = true;

    public const int MaxHP = 10;
    public const int MaxStamina = 20;

    public int food;
    public int HP;
    private int power;

    public bool useSword;
    public bool useBow;
    public bool arrowExist;
    public bool endBomb;

    public Button heartButton;
    public Button foodButton;
    public Button swordButton;
    public Button bowButton;
    public Button magicButton;

    public AudioClip[] clips;

    public bool death;

    public delegate void OnPlayerMoveStart(Vector2Int PlayerNewPos);
    public delegate void OnPlayerMoveEnd();

    public static OnPlayerMoveStart onMoveStart;
    public static OnPlayerMoveEnd onMoveEnd;

    public delegate void OnItemUse();

    public Dictionary<Item, float> inven { get; set; } = new Dictionary<Item, float>();
    private Dictionary<Item, OnItemUse> itemActions;

    private Vector2Int lookDir;

    [SerializeField]
    public GameObject deathEffect;
    
    protected override void Awake()
    {
        base.Awake();

        tileType = ETile.Player;

        enemyTile = ETile.Monster;

        enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
    }

    protected override void Start()
    {
        base.Start();

        stat.Init(10000, 1);

        lookDir = new Vector2Int(0, -1);

        //itemActions = new Dictionary<EItem, OnItemUse>
        //{
        //    { EItem.heal, () => { Mathf.Clamp(HP + 7, 0, MaxHP); inven[EItem.heal]--; } },
        //    { EItem.food, () => { Mathf.Clamp(food + 15, 0, MaxStamina); inven[EItem.food]--; } },
        //    { EItem.sword, () => { useSword = !useSword; useBow = false; } },
        //    { EItem.bow, () => { useBow = ! useBow; useSword = false; } },
        //    { EItem.magic, () => MagicAttack() }
        //};

        //horizontal = 0;
        //vertical = -1;

        blockingLayer = LayerMask.GetMask("Player");
    }

    void Update()
    {
        Control();

        animController.SetLookDirection(lookDir.x, lookDir.y);
    }

    public void Control()
    {
        if (!turn) return;

        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");

        Vector2Int inputVec = new Vector2Int(horizontal, vertical);
        
        if (!(horizontal == 0 && vertical == 0))
        {
            lookDir = inputVec;
            if (Input.GetKey(KeyCode.LeftControl))    // 시선 변경
                return;

            turn = false;

            bool successMove = Move(inputVec);
            
            if (!successMove)
                turn = true;
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            turn = false;

            StartCoroutine(Attack(lookDir));
        }
    }

    protected bool Move(Vector2Int dir)
    {
        animController.PlayAnimDirection(dir);
        
        Vector2Int start, movePos;

        start = mapPos;
        movePos = start + dir;

        ETile tileCondition = onGetTileCondition.Invoke(movePos);
        if (tileCondition == ETile.Wall || !MapManager.Instance.CanCrossWalk(start, dir))
        {
            return false;
        }
        else if (tileCondition == ETile.Empty)
        {
            mapPos = movePos;

            animController.PlayWalk();

            onMoveUnit(this, start, movePos);
            StartCoroutine(SmoothMovement(movePos));

            StartCoroutine(GameManager.instance.PlayerMoveEnd());
        }
        else if (tileCondition == enemyTile)
        {
            StartCoroutine(Attack(dir));
        }

        return true;
    }

    protected override IEnumerator Attack(Vector2Int dir)
    {
        animController.PlayAttack();

        yield return base.Attack(dir);

        yield return attackWait;

        yield return new WaitUntil(() => !attackDelay);

        yield return StartCoroutine(GameManager.instance.PlayerAttackEnd());
    }

    protected override void CheckAttack(Vector2 pos, Vector2Int dir)
    {
        if(MapManager.Instance.CanCrossWalk(mapPos, dir))
        {
            base.CheckAttack(pos, dir);
        }
    }

    protected override void Death()
    {

    }

    public void OnPlayerTurnStart()
    {
        turn = true;

        TakeItems();
    }

    public void TakeItems()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = Item.layer;
        filter.useTriggers = true;

        List<Collider2D> colliders = new List<Collider2D>();
        boxCol.OverlapCollider(filter, colliders);

        foreach (var col in colliders)
        {
            Item newItem = col.GetComponent<Item>();
            if (newItem)
            {
                inven.Add(newItem, newItem.chargeVal);
                Destroy(col.gameObject);
            }
        }
    }

    //protected override void OnTriggerStay2D(Collider2D collider)
    //{
    //    //base.OnTriggerStay2D(collider);
    //    //if (!turn) return;

    //    //Item newItem = collider.GetComponent<Item>();
    //    //if(newItem)
    //    //{
    //    //    inven.Add(newItem, newItem.chargeVal);
    //    //    Destroy(newItem.gameObject);
    //    //}
    //}

    public void UseItem(Item item)
    {
        inven[item]--;

        item.Use();
    }
}
