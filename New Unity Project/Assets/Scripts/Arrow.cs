using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    private GameObject player;
    private float speed;
    public AudioSource effect;


    public int rotation;

    void Awake()
    {
        rotation = 0;
        speed = 8f;
        player = GameObject.Find("Player");
        this.transform.position = player.transform.position;
    }

    private void Update()
    {
        if (rotation == 1)
            this.transform.Translate(new Vector3(1, 0) * speed * Time.deltaTime);
        else if (rotation == 2)
        {
            Debug.Log(rotation);
            this.transform.Translate(new Vector3(-1, 0) * speed * Time.deltaTime);
        }
        else
            this.transform.Translate(new Vector3(player.GetComponent<Player>().horizontal, player.GetComponent<Player>().vertical) * speed * Time.deltaTime);
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>().LoseHP(2);
            player.GetComponent<Player>().arrowExist = false;
            Destroy(this.gameObject);
        }
        else if (other.gameObject.CompareTag("Wall"))
        {
            player.GetComponent<Player>().arrowExist = false;
            Destroy(this.gameObject);
        }
    }

    //public void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (other.gameObject.CompareTag("Enemy"))
    //    {
    //        other.GetComponent<Enemy>().LoseHP(2);
    //        player.GetComponent<Player>().arrowExist = false;
    //        Destroy(this.gameObject);
    //    }
    //}
}
