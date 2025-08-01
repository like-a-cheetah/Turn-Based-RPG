using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject
{
    public Animator anim;
    private Transform target;
    private bool skipMove;

    public int power;
    public int HP;

    public const float SQRT2 = 1.4142135f;
    
    private static Dictionary<Enemy, Vector2> enemiesPos = new Dictionary<Enemy, Vector2>();

    public AIAroundDetector playerAround;
    public AIFollowDetector playerDetector;
    public AIEnemyDetector unitDetector;

    private Vector2[] dirs = new Vector2[]
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
    public GameObject item1;
    public GameObject item2;
    public GameObject item3;
    public GameObject item4;
    public int itemNum;
    
    public AudioClip[] clips;

    public GameObject floor;

    public delegate void OnDeathDelegate(Enemy enemy);

    public static OnDeathDelegate onDeath;

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

    protected override void AttemptMove<T>(int xDir, int yDir)
    {
        if (xDir == 0 && yDir == 0)
            anim.SetBool("ismove", false);
        else
            anim.SetBool("ismove", true);

        anim.SetFloat("inputX", xDir);
        anim.SetFloat("inputY", yDir);
        base.AttemptMove<T>(xDir, yDir);
    }

    public void Patrol()
    {
        Vector2 dir = RandomDirection();
        AttemptMove<Enemy>((int)dir.x, (int)dir.y);
    }

    public void Trace(Vector2 PlayerTargetPos)
    {

    }

    public void MoveEnemy() //이동 방향 정하는 함수
    {
        if (HP <= 0)
            return;

        int xDir = 0;
        int yDir = 0;
        int count = 0;

        if (playerDetector.PlayerDetected)  //AIfollower
        {
            if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)    //현재 x값이 같음(y값은 다르다는 것)
                yDir = target.position.y > transform.position.y ? 1 : -1;
            else    //현재 x값이 다름(y값은 같을 수 있다는 것)
            {
                if (Mathf.Abs(target.position.y - transform.position.y) < float.Epsilon)
                {
                    xDir = target.position.x > transform.position.x ? 1 : -1;
                }
                else
                {
                    xDir = target.position.x > transform.position.x ? 1 : -1;
                    yDir = target.position.y > transform.position.y ? 1 : -1;
                }
            }
            attackX = xDir;
            attackY = yDir;
            AttemptMove<Player>(xDir, yDir);
        }
        else
        {
            Vector2 direction = RandomDirection();
            xDir = (int)direction.x;
            yDir = (int)direction.y;
            prePosition = transform.position;   //이동하기 전의 위치 저장

            AttemptMove<Player>(xDir, yDir);    //이동

            while ((prePosition == transform.position) && count < 10)  //이동한 위치가 제자리라면
            {
                direction = RandomDirection(); //다시 방향 조정
                xDir = (int)direction.x;
                yDir = (int)direction.y;

                AttemptMove<Player>(xDir, yDir);

                count++;
            }
            if (count == 4 || prePosition == transform.position)
            {
                Debug.Log("움직이지 못함");
                transform.position = new Vector3(preX, preY);
            }
        }
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

    private Vector2 RandomDirection()
    {
        Vector2 resultDir = Vector2.zero;

        Vector2 start = transform.position;

        List<Vector2> tmpDirs = new List<Vector2>(dirs);

        while (tmpDirs.Count > 0)
        {
            int randN = Random.Range(0, tmpDirs.Count);
            Vector2 testDir = tmpDirs[randN];
            
            RaycastHit2D hit = Physics2D.Raycast(start, testDir, 1f, blockingLayer);
            if (hit.collider != null || enemiesPos.ContainsValue(start + testDir))
            {
                tmpDirs.Remove(testDir);
            }
            else
            {
                resultDir = tmpDirs[randN];
                break;
            }
        }

        enemiesPos[this] = start + resultDir;

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
        else if (itemNum == 1)
            DropItem1();
        else if (itemNum == 2)
            DropItem2();
        else if (itemNum == 3)
            DropItem3();
        else if (itemNum == 4)
            DropItem4();
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

    private void DropItem1()
    {
        var item = Instantiate<GameObject>(this.item1, floor.transform);
        item.transform.position = transform.position;
        item.SetActive(true);
    }

    private void DropItem2()
    {
        var item = Instantiate<GameObject>(this.item2, floor.transform);
        item.transform.position = transform.position;
        item.SetActive(true);
    }

    private void DropItem3()
    {
        var item = Instantiate<GameObject>(this.item3, floor.transform);
        item.transform.position = transform.position;
        item.SetActive(true);
    }

    private void DropItem4()
    {
        var item = Instantiate<GameObject>(this.item4, floor.transform);
        item.transform.position = transform.position;
        item.SetActive(true);
    }
}