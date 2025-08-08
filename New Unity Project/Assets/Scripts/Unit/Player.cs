using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MovingObject
{
    public GameManager GameManager;
    public GameObject arrow;
    public GameObject sideArrow;

    private BoxCollider2D collider;

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

    protected override void Awake()
    {
        tileType = ETile.Player;

        enemyTile = ETile.Monster;
    }

    protected override void Start()
    {
        tileType = ETile.Player;

        HP = 10;
        food = 20;
        power = 1;

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

        collider = GetComponent<BoxCollider2D>();

        blockingLayer = LayerMask.GetMask("Player");

        base.Start();
    }

    void Update()
    {        
        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");

        //대각선 방향으로 이동이 가능하게 할 수 있도록 프로젝트 세팅에서 텐키에서 home, pu, pd, end, insert로
        //horizontal, vertical의 값을 변경 가능하게 만듦,
        if (!(horizontal == 0 && vertical == 0) && moveEnd)
        {
            moveEnd = false;

            Vector2 moveDir = new Vector2(horizontal, vertical);
            
            RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDir, 1.0f, blockingLayer);
            if(hit.rigidbody != null) return;

            if (food == 0)  //포만감이 0일 경우 체력이 줄어듦
                HP--;
            else
                food--; //모든 동작시 포만감이 줄어듦

            bool successMove = Move(horizontal, vertical);

            Vector2Int targetPos = Vector2Int.RoundToInt(transform.position);
            targetPos.x += horizontal;
            targetPos.y += vertical;

            if (successMove)
            {
                onMoveStart.Invoke(targetPos);
            }
            else
            {
                moveEnd = true;
            }
        }
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
}
