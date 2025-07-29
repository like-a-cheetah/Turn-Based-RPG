using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour
{
    public float moveTime = 2f;
    public LayerMask blockingLayer;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;
    private float inverseMoveTime;

    private GameObject gameManager;

    //public BoxCollider2D stopBox;

    public Animator animator;
    
    public AudioSource radio;

    protected virtual void Start()
    {
        gameManager = GameObject.Find("GameManager");
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1f / moveTime;
        //radio.clip = gameManager.GetComponent<GameManager>().clips[0];
    }

    protected bool Move (int xDir, int yDir, out RaycastHit2D hit)
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
            StartCoroutine(SmoothMovement(end));
            return true;
        }

        return false;
    }

    protected IEnumerator SmoothMovement(Vector3 end)
    {
        float current = 0;
        float percent = 0;
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / moveTime;

            transform.position = Vector3.Lerp(transform.position, end, percent);

            yield return null;
        }
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