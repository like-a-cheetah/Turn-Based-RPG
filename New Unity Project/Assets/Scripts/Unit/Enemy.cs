using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Enemy : MovingObject
{
    //public Animator anim;

    public static List<Enemy> enemies = new List<Enemy>();

    public static HashSet<Enemy> patrolEnemies = new HashSet<Enemy>();
    public static HashSet<Enemy> traceEnemies = new HashSet<Enemy>();
    public static HashSet<Enemy> canAttackEnemies = new HashSet<Enemy>();

    private PathFindComponent pathFinder;
    private bool skipMove;

    private Player player { set; get; }

    public int power;
    public int HP;

    public const float SQRT2 = 1.4142135f;

    private static Dictionary<Enemy, Vector2> enemiesPos = new Dictionary<Enemy, Vector2>();
    
    public AIEnemyDetector detector;

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
        base.Awake();

        tileType = ETile.Monster;
        enemyTile = ETile.Player;

        enemies.Add(this);

        pathFinder = GetComponent<PathFindComponent>();

        detector = GetComponent<AIEnemyDetector>();
        detector.targetLayer = LayerMask.GetMask("Player");
        detector.OnPlayerDetected += (Player inObj) => { player = inObj; };
    }

    protected override void Start()
    {
        base.Start();

        stat.Init(1, 1);

        enemiesPos.Add(this, transform.position);
    }

    private void Update()
    {
    }

    public static void CheckAllEnemiesConditions()
    {
        canAttackEnemies.Clear();
        traceEnemies.Clear();
        patrolEnemies.Clear();

        foreach (var enemy in enemies)
        {
            enemy.CheckCondition();
        }
    }

    private void CheckCondition()
    {
        if (player)
        {
            Vector2Int playerPos = player.mapPos;
            
            if (CanAttack())
                canAttackEnemies.Add(this);
            else 
                traceEnemies.Add(this);
        }
        else
        {
            patrolEnemies.Add(this);
        }
    }

    protected IEnumerator Move(Vector2Int dir)
    {
        animController.PlayAnimDirection(dir);

        Vector2Int start, movePos;

        start = mapPos;
        movePos = start + dir;

        mapPos = movePos;

        animController.PlayWalk();

        onMoveUnit(this, start, movePos);

        yield return StartCoroutine(SmoothMovement(movePos));
    }

    public IEnumerator Patrol(Action onDone)
    {
        Vector2Int dir = RandomDirection();

        if (dir == Vector2Int.zero)
        {
            onDone?.Invoke();
            yield break;
        }
        
        yield return StartCoroutine(Move(dir));

        onDone?.Invoke();
    }

    public IEnumerator Trace(Action onDone)
    {
        pathFinder.PathFind(mapPos, player.mapPos);
        //if (!pathFinder.IsPathStillValid())
        //{
        //    pathFinder.PathFind(mapPos, playerObj.mapPos);
        //}

        if (pathFinder.paths.Count > 0)
        {
            Vector2Int targetPos = pathFinder.paths.Pop();
            Vector2Int currentPos = mapPos;
            Vector2Int dir = targetPos - currentPos;

            yield return StartCoroutine(Move(dir));
        }

        onDone?.Invoke();
    }

    public void LoseHP(int damage)
    {
        HP -= damage;
        Debug.Log(-damage);
    }

    protected override IEnumerator Attack(Vector2 dir)
    {
        animController.SetLookDirection(dir);

        yield return base.Attack(dir);
    }

    public IEnumerator AttackPlayer()
    {
        Vector2 dir = player.transform.position - transform.position;

        yield return StartCoroutine(Attack(dir));
    }

    public bool CanAttack()
    {
        Vector2Int start = mapPos;
        Vector2Int normal = player.mapPos - start;

        if (!MapManager.Instance.CanCrossWalk(start, normal) 
            || Mathf.Abs(normal.x) > 1 || Mathf.Abs(normal.y) > 1)
            return false;
        
        return true;
    }

    private Vector2Int RandomDirection()
    {
        Vector2Int resultDir = Vector2Int.zero;

        Vector2Int start = mapPos;

        List<Vector2Int> tmpDirs = new List<Vector2Int>(dirs);

        while (tmpDirs.Count > 0)
        {
            int randN = UnityEngine.Random.Range(0, tmpDirs.Count);
            Vector2Int testDir = tmpDirs[randN];

            Vector2Int targetPos = start + testDir;

            ETile targetTile = MapManager.Instance.GetTileType(targetPos);
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

    private void OnDestroy()
    {
        base.OnDestroy();

        onDeath.Invoke(this);

        enemies.Remove(this);
        canAttackEnemies.Remove(this);
        traceEnemies.Remove(this);
        patrolEnemies.Remove(this);
    }
}