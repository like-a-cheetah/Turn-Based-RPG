using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour
{
    private float moveTime = .4f;
    public LayerMask blockingLayer;

    private Animator anim;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;
    private float inverseMoveTime;

    private GameObject gameManager;

    //public BoxCollider2D stopBox;

    public Animator animator;
    public AnimationController animController;

    public AudioSource radio;

    protected Vector2 moveEndPos;

    protected bool moveEnd = true;

    private delegate ETile GetTileCondition(Vector2Int pos);
    private GetTileCondition onGetTileCondition;

    public delegate void OnMoveUnit(MovingObject unit, Vector2 moveDir);
    static public OnMoveUnit onMoveUnit;

    public delegate void OnStartAttack(Vector2Int dir);
    public OnStartAttack onStartAttack;

    public ETile tileType { get; protected set; }

    protected ETile enemyTile;

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        anim = GetComponent<Animator>();

        MapManager.Instance.SetTile(transform.position, tileType);
        onGetTileCondition += MapManager.Instance.GetTileType;

        gameManager = GameObject.Find("GameManager");
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        //radio.clip = gameManager.GetComponent<GameManager>().clips[0];

        blockingLayer = LayerMask.GetMask("Wall");
        
        animator = GetComponent<Animator>();
        animController = GetComponent<AnimationController>();
        animController.Initalize(animator);
    }

    protected virtual bool Move (Vector2 dir)
    {
        return Move((int)dir.x, (int)dir.y);
    }

    protected virtual bool Move (int xDir, int yDir)
    {
        Vector2Int start = Vector2Int.RoundToInt(transform.position);
        Vector2Int movePos = new Vector2Int(start.x + xDir, start.y + yDir);

        ETile tileCondition = onGetTileCondition.Invoke(movePos);
        if (tileCondition == ETile.Wall || !MapManager.Instance.CanCrossWalk(start, new Vector2Int(xDir, yDir)))
        {
            return false;
        }
        else if(tileCondition == ETile.Empty)
        {
            animController.PlayMoveAnim(xDir, yDir);

            onMoveUnit(this, new Vector2(xDir, yDir));
            StartCoroutine(SmoothMovement(movePos));
        }
        else if(tileCondition == enemyTile)
        {
            Attack(xDir, yDir);

            return false;
        }

        moveEndPos = movePos;

        return true;
    }

    protected IEnumerator SmoothMovement(Vector2 end)
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

        end.x = Mathf.RoundToInt(end.x);
        end.y = Mathf.RoundToInt(end.y);
        transform.position = end;
        moveEnd = true;
    }

    protected void Attack(int xDir, int yDir)
    {
        anim.SetBool("isAttack", true);

        //radio.clip = gameManager.GetComponent<GameManager>().clips[1];
        //radio.Play();
        
    }
}