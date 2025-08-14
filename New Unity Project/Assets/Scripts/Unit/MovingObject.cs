using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour
{
    private float moveTime = .4f;
    public LayerMask blockingLayer;
    protected LayerMask enemyLayer;

    private Animator anim;

    protected BoxCollider2D boxCol;
    private Rigidbody2D rb2D;
    private float inverseMoveTime;

    public string layerName;

    public Animator animator;
    public AnimationController animController;

    public AudioSource radio;

    protected Vector2 moveEndPos;

    protected delegate ETile GetTileCondition(Vector2Int pos);
    protected GetTileCondition onGetTileCondition;

    public delegate void OnMoveUnit(MovingObject unit, Vector2Int start, Vector2Int end);
    static public OnMoveUnit onMoveUnit;

    public delegate void OnStartAttack(Vector2Int dir);
    public OnStartAttack onStartAttack;

    public ETile tileType { get; protected set; }

    protected ETile enemyTile;

    public bool attacking { get; protected set; }

    protected SpriteRenderer sprite;

    protected float attackEndT = .2f;

    public WaitForSeconds attackWait = new WaitForSeconds(.5f);

    public Vector2Int mapPos { get; protected set; }

    protected Stat stat { get; set; }

    public bool attackDelay { get; protected set; }
    protected bool successAttack;

    private MovingObject killer;


    protected virtual void Awake()
    {
        mapPos = Vector2Int.RoundToInt(transform.position);
        
        anim = GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>();
        boxCol = GetComponent<BoxCollider2D>();

        animator = GetComponent<Animator>();
        animController = GetComponent<AnimationController>();

        stat = GetComponent<Stat>();
        stat.Init(1, 1);
        stat.onHpZero += animController.PlayDeath;

    }

    protected virtual void Start()
    {
        animController.Initalize(animator);

        MapManager.Instance.SetTile(mapPos, tileType);
        onGetTileCondition += MapManager.Instance.GetTileType;
        
        blockingLayer = LayerMask.GetMask("Wall");

        //radio.clip = gameManager.GetComponent<GameManager>().clips[0];

        //sprite = GetComponent<SpriteRenderer>();
    }

    protected virtual IEnumerator SmoothMovement(Vector2 end)
    {
        float current = 0;  // 누적 이동 시간
        float percent = 0;  // 전체 이동 시간 대비 진행 비율
        Vector2 start = transform.position;

        while (percent < 1f)
        {
            current += Time.deltaTime;  
            percent = current / moveTime;

            Vector2 movePos = Vector2.Lerp(start, end, percent);
            transform.position = movePos;

            yield return null;
        }
        
        transform.position = end;
    }

    protected IEnumerator SmoothMovement(Vector2 end, float newMoveTime)
    {
        float current = 0;  // 누적 이동 시간
        float percent = 0;  // 전체 이동 시간 대비 진행 비율
        Vector2 start = transform.position;

        while (percent < 1f)
        {
            current += Time.deltaTime;  
            percent = current / newMoveTime;

            Vector2 movePos = Vector2.Lerp(start, end, percent);
            transform.position = movePos;

            yield return null;
        }
        
        transform.position = end;
    }

    protected virtual IEnumerator Attack(Vector2Int dir)
    {
        attackDelay = true;

        Vector2 start = transform.position;
        Vector2 end = start + (Vector2)dir * 0.7f;
        
        yield return StartCoroutine(SmoothMovement(end, .2f));

        CheckAttack(start + dir, dir);

        yield return StartCoroutine(SmoothMovement(start, .2f));

        if (!successAttack) attackDelay = false;
    }

    protected virtual void CheckAttack(Vector2 pos, Vector2Int dir)
    {
        Vector2 size = new Vector2(.3f, .3f);
        Collider2D collider = Physics2D.OverlapBox(pos, size, 0, enemyLayer);
        if(collider)
        {
            MovingObject otherUnit = collider.GetComponent<MovingObject>();
            if (otherUnit && !otherUnit.CompareTag(tag))
            {
                successAttack = true;

                StartCoroutine(otherUnit.Attacked(this));
                otherUnit.stat.TakeDamage(stat.ad);

                return;
            }
        }
    }

    protected IEnumerator Attacked(MovingObject attacker)
    {
        if (stat.hp > 0)
            animController.PlayAttacked();

        killer = attacker;

        Vector2Int intDir = mapPos - attacker.mapPos;
        intDir.x = Mathf.Clamp(intDir.x, -1, 1);
        intDir.y = Mathf.Clamp(intDir.y, -1, 1);
        Vector2 dir = intDir;
        Vector2 end = dir * 0.3f + mapPos;

        yield return StartCoroutine(SmoothMovement(end, .2f));

        if (stat.hp > 0)
        {
            yield return StartCoroutine(SmoothMovement(mapPos, .2f));

            attacker.AttackFinish();
        }
    }

    //protected void OnTriggerEnter2D(Collider2D other)
    //{
    //    MovingObject otherUnit = other.GetComponent<MovingObject>();
    //    if (attacking || !other.CompareTag(tag))
    //    {
    //        Vector2 dir = transform.position - other.transform.position;
    //        dir.Normalize();

    //        otherUnit.stat.TakeDamage(stat.ad);

    //        return;
    //    }
    //}

    //protected abstract void OnTriggerEnter2D(Collider2D collision);

    protected virtual void OnTriggerStay2D(Collider2D other)
    {
        //Debug.Log(other.name);
        //if (!attacking) return;

        //MovingObject otherUnit = other.GetComponent<MovingObject>();
        //if (otherUnit && !otherUnit.CompareTag(tag))
        //{
        //    successAttack = true;
        //    attacking = false;

        //    StartCoroutine(otherUnit.Attacked(this));
        //    otherUnit.stat.TakeDamage(stat.ad);

        //    return;
        //}
    }

    public void AttackFinish()
    {
        successAttack = false;
        attackDelay = false;
    }

    protected virtual void Death()
    {
        killer.AttackFinish();

        Destroy(gameObject);
    }

    protected void OnDestroy()
    {
        MapManager.Instance.SetTile(mapPos, ETile.Empty);
    }
}