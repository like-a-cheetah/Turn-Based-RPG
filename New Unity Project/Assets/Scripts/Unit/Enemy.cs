using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject
{
    //public Animator anim;
    private PathFindComponent pathFinder;
    private Transform target;
    private bool skipMove;

    public int power;
    public int HP;

    public const float SQRT2 = 1.4142135f;

    private static Dictionary<Enemy, Vector2> enemiesPos = new Dictionary<Enemy, Vector2>();

    public AIAroundDetector playerAround;
    public AIFollowDetector playerDetector;
    public AIEnemyDetector unitDetector;

    public static readonly Vector2Int[] dirs = new Vector2Int[]
    {
        Vector2Int.up,   // 위
        Vector2Int.right,   // 오른쪽
        Vector2Int.down,  // 아래
        Vector2Int.left,   // 왼쪽

        new Vector2Int(1, 1),   // 우상
        new Vector2Int(1, -1),  // 우하
        new Vector2Int(-1, -1),  // 좌상
        new Vector2Int(-1, 1)   // 좌하
    };

    private float preX;
    private float preY;

    private float attackX;
    private float attackY;

    private bool canAttack { get; set; }

    private Vector3 prePosition;

    private BoxCollider2D collider;

    public int stun;
    public bool knockbacked { get; private set; }

    bool dampMove;

    Vector3 movetarget;

    public GameObject item0;
    public int itemNum;
    
    public AudioClip[] clips;

    public GameObject floor;

    public delegate void OnDeathDelegate(Enemy enemy);

    public static OnDeathDelegate onDeath;

    protected override void Awake()
    {
        tileType = ETile.Monster;
        GameManager.Instance.enemies.Add(this);

        pathFinder = GetComponent<PathFindComponent>();

        enemyTile = ETile.Player;
    }

    protected override void Start()
    {
        preX = transform.position.x;
        preY = transform.position.y;
        
        target = GameObject.FindGameObjectWithTag("Player").transform;
        base.Start();
        collider = GetComponent<BoxCollider2D>();

        itemNum = Random.Range(0, 5);

        enemiesPos.Add(this, transform.position);

        //blockingLayer = LayerMask.GetMask("Enemy");
    }

    private void Update()
    {
        Vector2 velo = Vector2.zero;

        if(stun != 0)
            this.GetComponent<SpriteRenderer>().color = Color.red;
        else if (GameManager.instance.playersTurn && stun == 0)
            this.GetComponent<SpriteRenderer>().color = new Color(150 / 255f, 150 / 255f, 150 / 255f);
        else
            this.GetComponent<SpriteRenderer>().color = Color.white;
        //anim.SetBool("ismove", false);

        if (dampMove)
        {
            if (HP > 0)
            {
                transform.position = Vector2.SmoothDamp(transform.position,
                    movetarget, ref velo, 0.08f);
                if (transform.position == movetarget)
                {
                    transform.position = new Vector2(movetarget.x, movetarget.y);
                    dampMove = false;
                }
            }
        }
    }

    public bool Patrol()
    {
        Vector2Int dir = RandomDirection();

        if (dir == Vector2Int.zero)
            return false;
        
        Move(dir.x, dir.y);

        return true;
    }

    public void Trace(Vector2 playerPos)
    {
        pathFinder.PathFind(Vector2Int.RoundToInt(transform.position), Vector2Int.RoundToInt(playerPos));
        //if (!pathFinder.IsPathStillValid())
        //{
        //    pathFinder.PathFind(Vector2Int.RoundToInt(transform.position), Vector2Int.RoundToInt(playerPos));
        //}

        if (pathFinder.paths.Count > 0)
        {
            Vector2 targetPos = pathFinder.paths.Pop();
            Vector2 currentPos = transform.position;
            Vector2 dir = targetPos - currentPos;
            Move(dir);
        }
    }

    public void LoseHP(int damage)
    {
        HP -= damage;
        Debug.Log(-damage);

        if (!knockbacked)
        {
            Debug.Log("넉백드");
            StartCoroutine(attacked(-attackX, -attackY));
        }
    }

    public bool CanAttack(Vector2Int targetPos)
    {
        Vector2Int start = Vector2Int.RoundToInt(transform.position);
        Vector2Int normal = targetPos - start;

        if (!MapManager.Instance.CanCrossWalk(start, normal) 
            || Mathf.Abs(normal.x) > 1 || Mathf.Abs(normal.y) > 1)
            return false;
        
        return true;
    }

    private Vector2Int RandomDirection()
    {
        Vector2Int resultDir = Vector2Int.zero;

        Vector2Int start = Vector2Int.RoundToInt(transform.position);

        List<Vector2Int> tmpDirs = new List<Vector2Int>(dirs);

        while (tmpDirs.Count > 0)
        {
            int randN = Random.Range(0, tmpDirs.Count);
            Vector2Int testDir = tmpDirs[randN];

            Vector2Int targetPos = start + testDir;

            ETile targetTile = MapManager.Instance.GetTileType(targetPos);
            //Debug.Log("TEST " + targetPos + " = " + targetTile);
            if (targetTile != ETile.Empty || !MapManager.Instance.CanCrossWalk(start, testDir))
            {
                tmpDirs.Remove(testDir);
            }
            else
            {
                resultDir = Vector2Int.RoundToInt(tmpDirs[randN]);
                break;
            }
        }

        return resultDir;
    }

    IEnumerator attacked(float xDir, float yDir)
    {
        if (HP <= 0)
        {
            HP = 0;
            yield return Death();
        }
        
        transform.Translate(new Vector2((float)xDir / 4, (float)yDir / 4));
        yield return new WaitForSecondsRealtime(0.35f);

        transform.Translate(new Vector2(-((float)xDir / 4), -((float)yDir / 4)));
        yield return new WaitForSecondsRealtime(0.35f);
    }

    public IEnumerator Death()
    {
        radio.clip = clips[1];
        radio.Play();
        this.GetComponent<SpriteRenderer>().color = Color.white;
        Vector3 where = transform.position;
        //anim.SetTrigger("death");

        Debug.Log("사망");

        yield return new WaitForSeconds(1.4f);
        if (itemNum == 0)
            DropItem0();
        itemNum = -1;

        this.GetComponent<BoxCollider2D>().enabled = false;

        this.gameObject.SetActive(false);

        onDeath.Invoke(this);

        enemiesPos.Remove(this);
    }

    public void BoomDamage()
    {
        stun = 2;
        knockbacked = true;
        Knockback();
    }

    private void Knockback()
    {
        Debug.Log("넉백");

        movetarget = transform.position + (-new Vector3(attackX, attackY) * 1);

        dampMove = true;

        LoseHP(2);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            Debug.Log("벽꿍");
            HP = 0;
            StartCoroutine(Death());
        }
    }

    private void DropItem0()
    {
        var item = Instantiate<GameObject>(this.item0, floor.transform);
        item.transform.position = transform.position;
        item.SetActive(true);
    }
}