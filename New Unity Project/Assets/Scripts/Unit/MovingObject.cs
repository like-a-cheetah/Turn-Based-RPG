using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour
{
    private float moveTime = .5f;
    public LayerMask blockingLayer;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;
    private float inverseMoveTime;

    private GameObject gameManager;

    //public BoxCollider2D stopBox;

    public Animator animator;
    
    public AudioSource radio;

    protected Vector2 moveEndPos;

    protected bool moveEnd = true;

    protected virtual void Start()
    {
        gameManager = GameObject.Find("GameManager");
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        //radio.clip = gameManager.GetComponent<GameManager>().clips[0];

        blockingLayer = LayerMask.GetMask("Wall");
    }

    protected virtual bool Move (int xDir, int yDir, out RaycastHit2D hit)
    {
        Vector2 start = transform.position;
        Vector2 end = start + new Vector2(xDir, yDir);
        
        boxCollider.enabled = false;
        hit = Physics2D.Linecast(start, end, blockingLayer);
        boxCollider.enabled = true;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(attack(xDir/2, yDir/2));
            return false;
        }

        if(hit.transform == null)
        {
            moveEndPos = end;
            StartCoroutine(SmoothMovement(end));
            return true;
        }

        moveEndPos = start;
        return false;
    }

    protected IEnumerator SmoothMovement(Vector3 end)
    {
        float current = 0;  // 누적 이동 시간
        float percent = 0;  // 전체 이동 시간 대비 진행 비율
        Vector3 start = transform.position;

        while (percent < 1f)
        {
            current += Time.deltaTime;  
            percent = current / moveTime;
            transform.position = Vector3.Lerp(start, end, percent);

            yield return null;
        }

        transform.position = end;
        moveEnd = true;
        Debug.Log(name + " : End");
    }

    protected virtual void AttemptMove <T> (int xDir, int yDir)
        where T : Component
    {
        RaycastHit2D hit;
        bool canMove = Move(xDir, yDir, out hit);

        if (hit.transform == null)
            return;
        
        T hitComoponent = hit.transform.GetComponent<T>();
        

        if (!canMove && hitComoponent != null)
        {
            OnCantMove(hitComoponent);
            StartCoroutine(attack(xDir, yDir));
        }
    }

    protected abstract void OnCantMove<T>(T component)
        where T : Component;

    protected IEnumerator attack(int xDir, int yDir)
    {
        radio.clip = gameManager.GetComponent<GameManager>().clips[1];
        radio.Play();

        transform.Translate(new Vector2((float)xDir / 2, (float)yDir / 2));

        yield return new WaitForSeconds(0.35f);

        transform.Translate(new Vector2(-((float)xDir / 2), -((float)yDir / 2)));

        yield return new WaitForSeconds(0.35f);
    }
}