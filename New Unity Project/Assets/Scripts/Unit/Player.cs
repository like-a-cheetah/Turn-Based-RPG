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
    private Animator anim;

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

    public delegate void OnPlayerMoveStart(Vector2 PlayerNewPos);
    public delegate void OnPlayerMoveEnd();

    public static OnPlayerMoveStart onMoveStart;
    public static OnPlayerMoveEnd onMoveEnd;

    public delegate void OnItemUse();

    private Dictionary<EItem, OnItemUse> itemActions;

    protected override void Start()
    {
        HP = 10;
        food = 20;
        power = 1;

        inven = new Dictionary<EItem, float>();

        itemActions = new Dictionary<EItem, OnItemUse>
        {
            { EItem.heal, () => { Mathf.Clamp(HP + 7, 0, MaxHP); inven[EItem.heal]--; } },
            { EItem.food, () => { Mathf.Clamp(food + 15, 0, MaxStamina); inven[EItem.food]--; } },
            { EItem.sword, () => { useSword = !useSword; useBow = false; } },
            { EItem.bow, () => { useBow = ! useBow; useSword = false; } },
            { EItem.magic, () => MagicAttack() }
        };

        horizontal = 0;
        vertical = -1;

        anim = GetComponent<Animator>();

        collider = GetComponent<BoxCollider2D>();

        blockingLayer = LayerMask.GetMask("Player");

        base.Start();
    }

    void Update()
    {
        anim.SetFloat("inputX", horizontal);
        anim.SetFloat("inputY", vertical);
        anim.SetBool("ismove", false);

        //if (HP <= 0)  //체력이 0이라면 결과 패널 출력
        //{
        //    GameManager.instance.playersTurn = false;
        //    if (!death) //사망 처리를 한 번만 일어나도록 함
        //    {
        //        animator.SetBool("death", true);
        //        death = true;

        //        radio.clip = clips[2];
        //        radio.Play();

        //        StartCoroutine(GameManager.result());
        //        return;
        //    }
        //}

        //if (useSword)   //검 장착시에는 공격력이 올라감
        //{
        //    power = 3;
        //    Debug.Log("무기 장착");
        //}
        //else
        //    power = 1;

        if (!GameManager.instance.playersTurn)  //플레이어의 턴일 경우 색이 바뀜
        {
            this.GetComponent<SpriteRenderer>().color = new Color(150 / 255f, 150 / 255f, 150 / 255f);
            return;
        }
        else
            this.GetComponent<SpriteRenderer>().color = Color.white;

        //LShift + 방향키를 누르는 방향으로 플레이어 캐릭터가 바라보게 된다.
        if (Input.GetKey(KeyCode.LeftShift) && (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.UpArrow) 
            || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.Home) 
            || Input.GetKey(KeyCode.End) || Input.GetKey(KeyCode.PageDown) || Input.GetKey(KeyCode.PageUp)))
        {
            horizontal = (int)Input.GetAxisRaw("Horizontal");
            vertical = (int)Input.GetAxisRaw("Vertical");
            return;
        }

        //대각선 방향으로 이동이 가능하게 할 수 있도록 프로젝트 세팅에서 텐키에서 home, pu, pd, end, insert로
        //horizontal, vertical의 값을 변경 가능하게 만듦,
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKey(KeyCode.Home) || Input.GetKey(KeyCode.End) || 
            Input.GetKey(KeyCode.PageDown) || Input.GetKey(KeyCode.PageUp) || Input.GetKey(KeyCode.UpArrow) || 
            Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.LeftArrow)) && moveEnd)
        {
            moveEnd = false;

            horizontal = (int)Input.GetAxisRaw("Horizontal");
            vertical = (int)Input.GetAxisRaw("Vertical");

            Vector2 moveDir = new Vector2(horizontal, vertical);
            
            RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDir, 1.0f, blockingLayer);
            if(hit.rigidbody != null) return;

            anim.SetFloat("inputX", horizontal);
            anim.SetFloat("inputY", vertical);
            
            if (food == 0)  //포만감이 0일 경우 체력이 줄어듦
                HP--;
            else
                food--; //모든 동작시 포만감이 줄어듦

            GameManager.instance.playersTurn = false;

            AttemptMove<Enemy>(horizontal, vertical);

            Vector2 targetPos = transform.position;
            targetPos.x += horizontal;
            targetPos.y += vertical;

            onMoveStart.Invoke(targetPos);
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

    protected override bool Move(int xDir, int yDir, out RaycastHit2D hit)
    {
        bool isMove = base.Move(xDir, yDir, out hit);

        if(isMove) onMoveStart.Invoke(moveEndPos);

        return isMove;
    }

    //public IEnumerator EnemyTurn()
    //{
    //    GameManager.instance.playersTurn = false;
    //    //몬스터 동작 시작
    //   // yield return StartCoroutine(GameManager.GetComponent<GameManager>().MoveEnemies());

    //    GameManager.instance.playersTurn = true;
    //}


    protected override void AttemptMove<T> (int xDir, int yDir)
    {
        if(!Input.GetKeyDown(KeyCode.Space))
            anim.SetBool("ismove", true);

        base.AttemptMove <T>(xDir, yDir);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {   //각 아이템을 줍는다. 플레이어 오브젝트의 자식에 다른 오브젝트도 있기 때문에 두번 동작하여
        //두번 동작한 값으로 개수가 올라감
        if (other.tag == "Exit1")
        {
            radio.clip = clips[3];  //레벨 오르는 사운드 출력
            radio.Play();

            GameManager.changeGrid = true;
            GameManager.NextFloor();
            Debug.Log("다음 단계");
        }
        else if (other.tag == "Exit2")
        {
            radio.clip = clips[3];  //레벨 오르는 사운드 출력
            radio.Play();

            GameManager.changeGrid = true;
            GameManager.NextFloor2();
            Debug.Log("다음 단계");
        }
        else if (other.tag == "Exit3")
        {
            radio.clip = clips[3];  //레벨 오르는 사운드 출력
            radio.Play();

            GameManager.changeGrid = true;
            GameManager.NextFloor3();
            Debug.Log("다음 단계");
        }
    }

    protected override void OnCantMove<T> (T component)
    {
        Enemy hitEnemey = component as Enemy;
        hitEnemey.LoseHP(power);
        Debug.Log("공격");

        anim.SetBool("ismove", false);

        anim.SetFloat("inputX", Input.GetAxisRaw("Horizontal"));
        anim.SetFloat("inputY", Input.GetAxisRaw("Vertical"));
    }

    private void Restart()  //출구 오브젝트와 충돌 할 때 호출(다음단계로 이동)
    {
        Debug.Log("Restart()");                         // loadedLevelName 사용해보기
        //Application.LoadLevel(Application.loadedLevel); //마지막으로 로드된 신을 로드한다는 의미, 아직 씬은 한개임
    }

    private void CheckIfGameOver()
    {
        if (food <= 0)
        {
         
        }
    }

    public void LoseHP(int damage, float damagedX, float damagedY)  //공격 받을때 loss값 만큼 감소
    {
        HP -= damage;

        StartCoroutine(attacked(damagedX, damagedY));
    }

    IEnumerator attacked(float xDir, float yDir)
    {
        if (HP <= 0)
        {
            //yield return new WaitForSecondsRealtime(0.7f);
            yield return null;
        }

        transform.Translate(new Vector2((float)xDir / 4, (float)yDir / 4));

        yield return new WaitForSecondsRealtime(0.35f);

        transform.Translate(new Vector2(-((float)xDir / 4), -((float)yDir / 4)));

        yield return new WaitForSecondsRealtime(0.35f);
    }

    public IEnumerator ShootArrow(int dirY)
    {
        radio.clip = clips[1];
        radio.Play();
        var shootingArrow = Instantiate<GameObject>(this.arrow);

        Vector3 direction = new Vector3(0, 0, 0); ;
        
        if (dirY == -1)  //위로 이동
        {
            shootingArrow.GetComponent<SpriteRenderer>().flipY = true;
        }

        arrowExist = true;

      //  bowPoint -= 1;
        
        GameManager.playersTurn = false;

        yield return new WaitWhile(() => arrowExist == true);

        GameManager.playersTurn = true;

       // yield return StartCoroutine(EnemyTurn());
    }

    public IEnumerator ShootSideArrow(int dirX)
    {
        radio.clip = clips[1];
        radio.Play();
        var shootingSideArrow = Instantiate<GameObject>(this.sideArrow);

        Vector3 direction = new Vector3(0, 0, 0); ;
        
        if (dirX == 1)  //오른쪽으로 이동
        {
            shootingSideArrow.GetComponent<SpriteRenderer>().flipX = true;
        }

        arrowExist = true;

       // bowPoint -= 1;

        GameManager.playersTurn = false;

        yield return new WaitWhile(() => arrowExist == true);

        GameManager.playersTurn = true;

    //    yield return StartCoroutine(EnemyTurn());
    }

    public IEnumerator ShootCross(int dirX, int dirY)
    {
        radio.clip = clips[1];
        radio.Play();
        var shootingSideArrow = Instantiate<GameObject>(this.sideArrow);

        if (dirX == 1)  //우측
        {
            shootingSideArrow.GetComponent<SpriteRenderer>().flipX = true;

            if (dirY == 1)  //우상향
                shootingSideArrow.transform.localEulerAngles = new Vector3(0, 0, 45);

            else    //우하향
                shootingSideArrow.transform.localEulerAngles = new Vector3(0, 0, -45);

            shootingSideArrow.GetComponent<Arrow>().rotation = 1;
        }
        else    //좌측
        {
            if (dirY == 1)  //좌상향
                shootingSideArrow.transform.localEulerAngles = new Vector3(0, 0, -45);

            else    //좌하향
                shootingSideArrow.transform.localEulerAngles = new Vector3(0, 0, 45);

            shootingSideArrow.GetComponent<Arrow>().rotation = 2;
        }

        arrowExist = true;

        //inven[bowPoint -= 1;

        GameManager.playersTurn = false;

        yield return new WaitWhile(() => arrowExist == true);

     //   yield return StartCoroutine(EnemyTurn());
    }

    public IEnumerator MagicAttack()
    {
        GameManager.playersTurn = false;

        yield return new WaitWhile(() => endBomb == false);

      //  yield return StartCoroutine(EnemyTurn());
    }

    public void revive()
    {
        HP = 10;
        food = 20;
        animator.SetBool("death", false);
        death = false;
        GameManager.instance.playersTurn = true;
    }
}
