/*
        작성자 : 20184009 김주원
        프로그램 명 : 성 탈출 전략 게임
        장르 : 2D기반 로그라이크
        프로그램 설명 : MovingObject라는 부모 클래스의 이동 함수를
                       플레이어와 몬스터의 추상함수 형태로 만들었고
                       가장 중요한 부분은, 각각 차례대로 한 칸씩
                       이동하여 상호작용 하는 게임이기 때문에 몬스터의
                       리스트를 만든 후, 코루틴 함수로 차례대로 이동하여
                       겹치지 않도록 만들었다.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GameManager : MonoBehaviour
{
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
    public List<Enemy> enemies;

    public Player player;

    public float delay;

    public AudioClip[] clips;
    public AudioSource radio;

    public GameObject resultPanel;
    public GameObject VictoryPanel;

    public Vector2 StartPos;

    void Awake()
    {
        floorLevel = 1;

        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        
        enemies = new List<Enemy>();
        enemies = FindObjectsOfType<Enemy>().ToList();

        Enemy.onDeath += EnemyDelete;
    }

    private void Start()
    {
        Player.onMoveStart += PlayerStartMove;
        //Player.onMoveEnd += PlayerStartMove;
    }

    private void Update()
    {
    }

    void InitGame()
    {
        enemies.Clear();
    }

    private void EnemyDelete(Enemy deathEnemy)
    {
        enemies.Remove(deathEnemy);
    }

    private void PlayerStartMove(Vector2 PlayerTargetPos)
    {
        List<Enemy> CanAttackEnemies = new List<Enemy>();

        foreach(var enemy in enemies)
        {
            if(enemy.playerDetector.PlayerDetected)
            {
                if(!enemy.CanAttack(PlayerTargetPos))   //추적
                {
                    //enemy.Trace(PlayerTargetPos);
                }
                else
                {
                    CanAttackEnemies.Add(enemy);
                }
            }
            else
            {
                enemy.Patrol();
            }            
        }

        foreach (var enemy in CanAttackEnemies) //공격
        {
           // enemy.AttemptMove<Enemy>();
        }

        playersTurn = true;
    }

    //public IEnumerator MoveEnemies()
    //{
        //bool detected = false;
        //bool delay = false;
        //int lastEnemy;
        //bool isEnemiesEnable = false;

        //playersTurn = false;

        //for (int i = 0; i < enemies.Count; i++)
        //{
        //    if (enemies[i].isActiveAndEnabled)
        //    {
        //        if(enemies[i].playerDetector.PlayerDetected)
        //            detected = true;

        //        if (enemies[i].HP <= 0)
        //        {
        //            yield return StartCoroutine(enemies[i].Death());
        //        }
        //        isEnemiesEnable = true;
        //    }
        //}

        //if (!isEnemiesEnable)
        //{
        //    Debug.Log("적 전멸");
        //    yield return new WaitForSeconds(0.4f);
        //    yield break;
        //}
        //else
        //    yield return new WaitForSeconds(0.5f);

        //lastEnemy = 0;

        //for (int i = 0; i< enemies.Count; i++)
        //{
        //    if (enemies[i].isActiveAndEnabled && enemies[i].stun == 0)
        //    {
        //        enemies[i].knockbacked = false;
        //        if (!enemies[i].unitDetector.PlayerDetected)    //유닛과 겹칠 수 없음,
        //        {
        //            enemies[i].MoveEnemy(); //딜레이 없이 바로 이동
        //        }
        //        else    //유닛과 겹칠 수 있음
        //        {
        //            if (enemies[i].playerDetector.PlayerDetected && !delay) //플레이어를 발견
        //            {
        //                yield return new WaitForSeconds(player.GetComponent<Player>().moveTime + 0.2f);
        //                enemies[i].MoveEnemy();
        //                delay = true;
        //            }
        //            else
        //            {
        //                yield return new WaitForSeconds(enemies[lastEnemy].moveTime + 0.001f);
        //                enemies[i].MoveEnemy();
        //            }
        //        }
        //        lastEnemy = i;
        //    }
        //    else if (enemies[i].stun > 0)
        //    {
        //        enemies[i].stun--;
        //    }
        //}

        //yield return new WaitForSeconds(0.5f);

        //playersTurn = true;
    //}

    public void NextFloor() //매개변수로 층수 받아와서 층수 늘려가면서 변환시키기 구현하기
    {
        changeGrid = !changeGrid;

        f1.SetActive(false);
        f2.SetActive(true);
        
        enemies = new List<Enemy>();
        InitGame();
    }

    public void NextFloor2() //매개변수로 층수 받아와서 층수 늘려가면서 변환시키기 구현하기
    {
        changeGrid = !changeGrid;
        
        f2.SetActive(false);
        f3.SetActive(true);
        floorLevel += 0.5f;

        enemies = new List<Enemy>();
        InitGame();
    }

    public void NextFloor3() //매개변수로 층수 받아와서 층수 늘려가면서 변환시키기 구현하기
    {
        changeGrid = !changeGrid;
        
        VictoryPanel.SetActive(true);
        return;
    }

    public IEnumerator result()
    {
        radio.Stop();
        yield return new WaitForSeconds(1f);
        player.GetComponent<SpriteRenderer>().color = new Color(0,0,0,0f);
        resultPanel.SetActive(true);
    }

    public void restart()
    {
        Debug.Log("재시작");
        radio.clip = clips[0];
        radio.Play();
        resultPanel.SetActive(false);

        player.GetComponent<Player>().revive();
    }

    public void quitGame()
    {
        Application.Quit();
    }
}
