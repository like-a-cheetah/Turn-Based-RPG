using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine;

public class Player : MovingObject
{
    public GameManager GameManager;
    public GameObject arrow;
    public GameObject sideArrow;

    public int horizontal;
    public int vertical;

    public const int MaxHP = 10;
    public const int MaxStamina = 20;

    public int food;
    public int HP;
    private int power;

    public Dictionary<EItem, float> inven { get; set; }

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

    private Dictionary<EItem, OnItemUse> itemActions;

    private Vector2Int lookDir;

    [SerializeField]
    public GameObject deathEffect;
    
    protected override void Awake()
    {
        base.Awake();

        tileType = ETile.Player;

        enemyTile = ETile.Monster;
    }

    protected override void Start()
    {
        base.Start();

        stat.Init(10000, 1);

        lookDir = new Vector2Int(0, -1);

        tileType = ETile.Player;

        inven = new Dictionary<EItem, float>();

        //itemActions = new Dictionary<EItem, OnItemUse>
        //{
        //    { EItem.heal, () => { Mathf.Clamp(HP + 7, 0, MaxHP); inven[EItem.heal]--; } },
        //    { EItem.food, () => { Mathf.Clamp(food + 15, 0, MaxStamina); inven[EItem.food]--; } },
        //    { EItem.sword, () => { useSword = !useSword; useBow = false; } },
        //    { EItem.bow, () => { useBow = ! useBow; useSword = false; } },
        //    { EItem.magic, () => MagicAttack() }
        //};

        horizontal = 0;
        vertical = -1;

        blockingLayer = LayerMask.GetMask("Player");

        sprite = GetComponent<SpriteRenderer>();
        base.Start();
    }

    void Update()
    {
        Control();

        animController.SetLookDirection(lookDir.x, lookDir.y);
    }

    public void ItemCharging(EItem newItem, float chargeVal)
    {
        inven[newItem] += chargeVal;
    }

    public void UseItem(EItem useItem)
    {
        if(inven[useItem] > 0)
        {
            itemActions[useItem].Invoke();
        }
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

            Instantiate(deathEffect, transform.position, Quaternion.identity);
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

    protected override IEnumerator Attack(Vector2 dir)
    {
        yield return base.Attack(dir);

        yield return attackWait;

        yield return new WaitUntil(() => !attackDelay);

        yield return StartCoroutine(GameManager.instance.PlayerAttackEnd());
    }

    protected override void Death()
    {

    }
}
