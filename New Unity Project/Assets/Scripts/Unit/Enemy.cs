using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject
{
    public Animator anim;
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

    public static readonly Vector2[] dirs = new Vector2[]
    {
        new Vector2(0, 1),   // 위
        new Vector2(1, 0),   // 오른쪽
        new Vector2(0, -1),  // 아래
        new Vector2(-1, 0),   // 왼쪽

        new Vector2(1, 1),   // 우상
        new Vector2(1, -1),  // 우하
        new Vector2(-1, -1),  // 좌상
        new Vector2(-1, 1)   // 좌하
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

        blockingLayer = LayerMask.GetMask("Enemy");
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
        anim.SetBool("ismove", false);

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
    }

    protected override void OnCantMove<T>(T component) //점거중인 공건에 적이 이동하려 할때 호출
    {
        Player hitPlayer = component as Player;

        hitPlayer.LoseHP(power, attackX, attackY);
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

    public bool CanAttack(Vector2 targetPos)
    {
        if(!(Vector2.Distance(targetPos, transform.position) <= SQRT2)) return false;

        Vector2 start = transform.position;
        foreach (Vector2 dir in dirs)
        {
            RaycastHit2D hit = Physics2D.Raycast(start, dir, 1f, blockingLayer);
            if (canAttack = hit.collider.tag == "Player")
                return true;
        }

        return false;
    }

    private Vector2Int RandomDirection()
    {
        Vector2Int resultDir = Vector2Int.zero;

        Vector2 start = transform.position;

        List<Vector2> tmpDirs = new List<Vector2>(dirs);

        while (tmpDirs.Count > 0)
        {
            int randN = Random.Range(0, tmpDirs.Count);
            Vector2 testDir = tmpDirs[randN];

            Vector2 targetPos = start + testDir;

            ETile targetTile = MapManager.Instance.GetTileType(targetPos);
            //Debug.Log("TEST " + targetPos + " = " + targetTile);
            if (targetTile != ETile.Empty || !MapManager.Instance.CanCrossWalk(start, testDir))
            {
                tmpDirs.Remove(testDir);
            }
            else
            {
                resultDir = Vector2Int.FloorToInt(tmpDirs[randN]);
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
        anim.SetTrigger("death");

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