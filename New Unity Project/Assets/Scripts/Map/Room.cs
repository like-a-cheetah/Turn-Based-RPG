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

    public List<Entrance> entrances;

    private bool isEndRoomReplace;

    public class Entrance
    {
        public Vector2 dir;
        public Vector2 pos;
        
        public Entrance(Vector2 inDir, Vector2 inPos)
        {
            dir = inDir;
            pos = inPos;
        }
    }
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        collider = GetComponent<BoxCollider2D>();

        entrances = new List<Entrance>();
    }

    void Start()
    {
    }

    void Update()
    {
        if(isEndRoomReplace)
            DebugEntrance();
    }

    public void Initialize()
    {
        OverlapCheckColliderMake();

        //CreateWall();
    }

    public void EndReplaceRoom()
    {
        GenerateRandObstacle();
        SetEntrance();

        isEndRoomReplace = true;
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

        Vector2Int start = Vector2Int.RoundToInt(transform.position);

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

    private void GenerateRandObstacle()
    {
        if (width < 3 || height < 3)
            return;

        int insideArea = (width * height) - (width * 2 + (height - 2) * 2);
        if (insideArea >= 1)
        {
            int randObstacleN = Random.Range(0, insideArea + 1);

            for (int i = 0; i < randObstacleN; i++)
            {
                Vector2Int start = Vector2Int.RoundToInt(transform.position);
                Vector2Int end = start;
                end.x += width;
                end.y += height;

                int tryN = 0;

                int x, y;
                Vector2 obstaclePos;
                do
                {
                    x = Random.Range(start.x + 1, end.x - 1);
                    y = Random.Range(start.y + 1, end.y - 1);

                    obstaclePos = new Vector2(x, y);

                    tryN++;
                } while (MapManager.Instance.GetTileType(obstaclePos) != ETile.Wall && tryN <= 10);

                Instantiate(wall, obstaclePos, Quaternion.identity, transform);
                MapManager.Instance.SetTile(obstaclePos, ETile.Wall);
            }
        }
    }

    private void SetEntrance()
    {
        bool isSetEntrance = false;
        int min = 0, max = 0, defaultIndex = 0;

        for (int dir = 0; dir < 4; dir++)
        {
            if (Random.Range(0, 2) == 1)
            {
                isSetEntrance = true;

                SetEntranceRangeFromDirection(dir, out min, out max, out defaultIndex);

                CreateEntranceInRange(dir, min, max, defaultIndex);
            }
        }

        if (!isSetEntrance)
        {
            int dir = Random.Range(0, 4);

            SetEntranceRangeFromDirection(dir, out min, out max, out defaultIndex);

            CreateEntranceInRange(dir, min, max, defaultIndex);
        }
    }

    private void SetEntranceRangeFromDirection(int dir, out int min, out int max, out int defaultIndex)
    {
        Vector2Int offset = Vector2Int.FloorToInt(transform.position);

        min = 0;
        max = 0;
        defaultIndex = 0;

        switch (dir) //입구를 만들 벽의 방향
        {
            case 0: //상
                min = offset.x;
                max = min + width;
                defaultIndex = offset.y + height - 1;
                break;

            case 1: //우
                min = offset.y;
                max = min + height;
                defaultIndex = offset.x + width - 1;
                break;

            case 2: //하
                min = offset.x;
                max = min + width;
                defaultIndex = offset.y;
                break;

            case 3: //좌
                min = offset.y;
                max = min + height;
                defaultIndex = offset.x;
                break;
        }
    }

    private void CreateEntranceInRange(int dir, int min, int max, int defaultIndex)
    {
        int maxCreatedEntranceN = Random.Range(1, 4);
        int createdEntranceN = 0;

        while (createdEntranceN == 0)
        {
            if (dir == 0 || dir == 2)
            {
                int y = defaultIndex;
                for (int x = min; x < max && createdEntranceN < maxCreatedEntranceN; x++)
                {
                    if (Random.Range(0, 2) == 1)
                    {
                        Vector2 dirVec = dir == 0 ? new Vector2(0, 1) : new Vector2(0, -1);

                        Entrance newEntrance = new Entrance(dirVec, new Vector2(x, y));
                        entrances.Add(newEntrance);

                        x++;
                        createdEntranceN++;
                    }
                }
            }
            else
            {
                int x = defaultIndex;
                for (int y = min; y < max && createdEntranceN < maxCreatedEntranceN; y++)
                {
                    if (Random.Range(0, 2) == 1)
                    {
                        Vector2 dirVec = dir == 1 ? new Vector2(1, 0) : new Vector2(-1, 0);

                        Entrance newEntrance = new Entrance(dirVec, new Vector2(x, y));
                        entrances.Add(newEntrance);

                        y++;
                        createdEntranceN++;
                    }
                }
            }
        }
    }

    private void DebugEntrance()
    {
        foreach (var entrance in entrances)
        {
            Vector2 start = entrance.pos;
            Vector2 end = start + entrance.dir;

            Debug.DrawLine(start, end);
        }
    }
}
