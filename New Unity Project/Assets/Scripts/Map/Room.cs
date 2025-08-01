using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public GameObject wall;
    public GameObject road;

    private Rigidbody2D rb;
    private BoxCollider2D collider;

    public int width;
    public int height;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        collider = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
    }

    void Update()
    {

    }

    public void Initialize()
    {
        OverlapCheckColliderMake();

        CreateWall();
    }

    private void OverlapCheckColliderMake()
    {
        collider.size = new Vector2(width, height);
        collider.offset = new Vector2((float)width / 2 - 0.5f, (float)height / 2 - 0.5f);
    }

    private void CreateWall()
    {
        Vector2 startPos = transform.position;

        for (int i = -1; i < width + 1; i++)
        {
            Vector2 spawnPos = startPos + new Vector2(i, -1);
            Instantiate(wall, spawnPos, Quaternion.identity, this.transform);
        }
        for (int i = -1; i < width + 1; i++)
        {
            Vector2 spawnPos = startPos + new Vector2(i, height);
            Instantiate(wall, spawnPos, Quaternion.identity, this.transform);
        }

        for (int i = -1; i < height + 1; i++)
        {
            Vector2 spawnPos = startPos + new Vector2(-1, i);
            Instantiate(wall, spawnPos, Quaternion.identity, this.transform);
        }
        for (int i = -1; i < height + 1; i++)
        {
            Vector2 spawnPos = startPos + new Vector2(width, i);
            Instantiate(wall, spawnPos, Quaternion.identity, this.transform);
        }
    }

    public void CreateRoom(ETile[,]map)
    {
        Vector2 startPos = transform.position;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector2 spawnPos = startPos + new Vector2(i, j);
                Instantiate(road, spawnPos, Quaternion.identity, this.transform);

                map[(int)spawnPos.y, (int)spawnPos.x] = ETile.Empty;
            }
        }
    }

    public bool GetIsTrigger()
    {
        ContactFilter2D filter = new ContactFilter2D().NoFilter();
        filter.useTriggers = true;

        Collider2D[] results = new Collider2D[10];

        Physics2D.SyncTransforms();

        return collider.OverlapCollider(filter, results) > 0;
    }

    public Vector2 GetRandomTilePos(ETile[,] map)
    {
        Vector2 randomPos = new Vector2(-1, -1);

        Vector2Int start = Vector2Int.FloorToInt(transform.position);

        List<Vector2Int> EmptyList = new List<Vector2Int>();
        for (int x = start.x; x < start.x + width; x++)
        {
            for (int y = start.y; y < start.y + height; y++)
            {
                if(map[y, x] == ETile.Empty)
                {
                    EmptyList.Add(new Vector2Int(x, y));
                }
            }
        }

        if(EmptyList.Count > 0) randomPos = EmptyList[Random.Range(0, EmptyList.Count)];

        return randomPos;
    }
}
