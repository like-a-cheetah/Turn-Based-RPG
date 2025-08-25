using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private float floorLevel;

    public GameObject f1;
    public GameObject f2;
    public GameObject f3;

    public bool changeGrid;

    public float turnDelay = 0.1f;

    public static GameManager instance = null;
    
    public bool enterStair = false;
    [HideInInspector] public bool playersTurn = true;
    
    private bool enemiesMoving;
    
    public Player player;

    public float delay;

    public AudioClip[] clips;
    public AudioSource radio;

    public GameObject resultPanel;
    public GameObject VictoryPanel;

    public Vector2 StartPos;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 중복 방지
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        floorLevel = 1;

        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        Enemy.onDeath += EnemyDelete;

        Item.Init();
    }

    private void Start()
    {
        player = FindObjectOfType<Player>();
    }

    private void Update()
    {
    }

    void InitGame()
    {
    }

    private void EnemyDelete(Enemy deathEnemy)
    {
    }

    public IEnumerator PlayerMoveEnd()
    {
        Enemy.CheckAllEnemiesConditions();

        yield return StartCoroutine(NotAttackEnemiesMove());

        if (Enemy.canAttackEnemies.Count > 0)
        {
            yield return new WaitForSeconds(0.5f);

            yield return StartCoroutine(EnemiesAttackStart());
        }

        player.OnPlayerTurnStart();
    }

    public IEnumerator PlayerAttackEnd()
    {
        Enemy.CheckAllEnemiesConditions();

        yield return StartCoroutine(EnemiesAttackStart());

        yield return StartCoroutine(NotAttackEnemiesMove());

        player.OnPlayerTurnStart();

    }

    public IEnumerator NotAttackEnemiesMove()
    {
        int remaining = 0;

        foreach (var enemy in Enemy.traceEnemies)
        {
            StartCoroutine(enemy.Trace(() => remaining--));
            remaining++;
        }

        Stack<Enemy> patrolFailedEnemies = new Stack<Enemy>();
        
        foreach (var enemy in Enemy.patrolEnemies)
        {
            StartCoroutine(enemy.Patrol(() => remaining--));
            remaining++;
        }

        //while (patrolFailedEnemies.Count != 0)
        //{
        //    Enemy failedEnemy = patrolFailedEnemies.Pop();
        //    StartCoroutine(failedEnemy.Patrol());
        //    remaining++;
        //}

        yield return new WaitUntil(() => remaining == 0);
    }

    public IEnumerator EnemiesAttackStart()
    {
        foreach (var enemy in Enemy.canAttackEnemies) //공격
        {           
            yield return StartCoroutine(enemy.AttackPlayer());

            yield return new WaitUntil(() => !enemy.attackDelay);

            yield return enemy.attackWait;
        }
    }
}
