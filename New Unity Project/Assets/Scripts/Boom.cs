using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boom : MonoBehaviour
{
    private GameObject player;
    public Animator anim;

    private void Awake()
    {
        player = GameObject.Find("Player");
        this.transform.position = player.transform.position;
        Debug.Log(this.transform.position);
    }

    private void Update()
    {
        this.gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy") 
            && !other.GetComponent<Enemy>().knockbacked)
        {
            other.GetComponent<Enemy>().BoomDamage();
        }
    }

    public IEnumerator Remove()
    {
        //this.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameObject.GetComponent<CircleCollider2D>().enabled = false;

        yield return new WaitForSeconds(2f);
        player.GetComponent<Player>().endBomb = true;
        Destroy(this.gameObject);
    }
}
