using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    private ETile[,] map;

    public GameObject wall;
    public GameObject road;

    [SerializeField] public Room room;
    [SerializeField] public Player playerPrefab;
    private List<Room> rooms;

    private int floor = 1;

    private int mapHeight = 56;
    private int mapWidth = 56;

    private const int roomMinLine = 1;
    private const int roomMaxLine = 10;

    private int[] roomMaxN = { 4, 6, 8 };
    private int[] enemyMaxN = { 5, 8, 12 };
    private int roomMinN = 4;
    private int roomN;
    
    private GameObject gameMap;

    const int maxRoomMakeAttempts = 100;

    [SerializeField] public Enemy[] enemiesPF;

    private List<Entrance> allEntrances;
    private List<(Entrance, Entrance)> entrancesPair;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 중복 방지
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        map = new ETile[mapHeight, mapWidth];
        rooms = new List<Room>();

        gameMap = new GameObject("RoomContainer");

        roomN = Random.Range(roomMinN, roomMaxN[floor] + 1);

        allEntrances = new List<Entrance>();
    }

    void Start()
    {
        Enemy.onDeath += TileClear;

        GenerateRooms();

        ConnectEntrances();

        CreatePlayer();

        GeneratesEnemies();
        
        MovingObject.onMoveUnit += (MovingObject unit, Vector2Int start, Vector2Int end) =>
        {
            TileClear(start);
            SetTile(end, unit.tileType);
        };
    }

    void Update()
    {
        DebugMapSpace();
    }

    public void TileClear(Enemy enemy)
    {
        TileClear(Vector2Int.RoundToInt(enemy.mapPos));
    }

    public void TileClear(Vector2Int pos)
    {
        Vector2Int clearPos = Vector2Int.RoundToInt(pos);

        int rows = map.GetLength(0);
        int cols = map.GetLength(1);

        if (map == null || clearPos.y < 0 || clearPos.y >= rows || clearPos.x < 0 || clearPos.x >= cols)
        {
            Debug.LogError("타일 청소 실패");
            return;
        }

        map[clearPos.y, clearPos.x] = ETile.Empty;
    }

    public void SetTile(Vector2Int pos, ETile newTileCondition)
    {
        Vector2Int clearPos = Vector2Int.RoundToInt(pos);

        int rows = map.GetLength(0);
        int cols = map.GetLength(1);

        if (map == null || clearPos.y < 0 || clearPos.y >= rows || clearPos.x < 0 || clearPos.x >= cols)
        {
            Debug.LogError("타일 청소 실패");
            return;
        }

        map[clearPos.y, clearPos.x] = newTileCondition;
    }

    public bool IsInMapBounds(Vector2 pos)
    {
        if (pos.x < 0 || pos.y < 0 || pos.x >= mapWidth || pos.y >= mapHeight)
            return false;
        return true;
    }

    public ETile GetTileType(Vector2Int pos)
    {
        return map[pos.y, pos.x];
    }

    public bool CanCrossWalk(Vector2Int startPos, Vector2Int dir)
    {
        if (dir.x == 0 || dir.y == 0)
            return true;

        for (int i = 0; i < 2; i++)
        {
            Vector2Int tmp = dir;
            tmp[i] = 0;

            Vector2Int end = startPos + tmp;
            ETile tile = MapManager.Instance.GetTileType(end);
            if (tile == ETile.Wall)
                return false;
        }

        return true;
    }

    private void GenerateRooms()
    {
        for (int i = 0; i < roomN; i++)
        {
            Room newRoom = InstantiateRandomRoom(i);
            rooms.Add(newRoom);

            Vector2Int startVector = new Vector2Int(newRoom.width, newRoom.height);
            Vector2Int endVector = new Vector2Int(mapWidth - newRoom.width - 1, mapHeight - newRoom.height - 1);
            
            newRoom.Initialize();

            int count = TryPlaceRoom(newRoom, startVector, endVector);

            if (count >= maxRoomMakeAttempts)
            {
                Destroy(newRoom.gameObject);

                continue;
            }

            newRoom.CreateRoom(map);
            newRoom.EndReplaceRoom();
        }
    }

    private Room InstantiateRandomRoom(int roomN)
    {
        int roomWidth = Random.Range(roomMinLine, roomMaxLine);
        int roomHeight = Random.Range(roomMinLine, roomMaxLine);

        Room newRoom = Instantiate<Room>(room, transform.position, Quaternion.identity, gameMap.transform);
        newRoom.width = roomWidth;
        newRoom.height = roomHeight;
        newRoom.roomN = roomN;

        return newRoom;
    }

    private int TryPlaceRoom(Room newRoom, Vector2Int startVector, Vector2Int endVector)
    {
        int count = 0;

        do
        {
            int x = Random.Range(startVector.x, endVector.x + 1);
            int y = Random.Range(startVector.y, endVector.y + 1);
            newRoom.transform.position = new Vector2(x, y);
            count++;
        }
        while (newRoom.GetIsTrigger() && count < maxRoomMakeAttempts);

        return count;
    }

    private void DebugMapSpace()
    {
        if (map == null) return;

        int rows = map.GetLength(0);
        int cols = map.GetLength(1);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (map[y, x] != ETile.Empty)
                {
                    Vector3 center = new Vector3(x, y, 0);
                    float size = 0.25f;

                    Debug.DrawLine(center - Vector3.right * size, center + Vector3.right * size, Color.red);
                    Debug.DrawLine(center - Vector3.up * size, center + Vector3.up * size, Color.red);
                }
                else if (map[y, x] == ETile.Player)
                {
                }
            }
        }
    }

    private void CreatePlayer()
    {
        int startRoomN = Random.Range(0, roomN);

        Vector2Int startPos = rooms[startRoomN].GetRandomTilePos(map);
        SetTile(startPos, ETile.Player);

        Instantiate<Player>(playerPrefab, new Vector2(startPos.x, startPos.y), Quaternion.identity, gameMap.transform);
    }

    private void GeneratesEnemies()
    {
        int enemyN = enemyMaxN[floor];

        for(int i=0; i<enemyN; i++)
        {
            Vector2Int randPos;
            ETile tile;

            do
            {
                int randRoomN = Random.Range(0, roomN);

                randPos = rooms[randRoomN].GetRandomTilePos(map);

                tile = GetTileType(randPos);
            }
            while (randPos == new Vector2Int(-1, -1) || tile == ETile.Monster);

            Enemy newEnemy = Instantiate<Enemy>(enemiesPF[0], (Vector2)randPos, Quaternion.identity, gameMap.transform);
            SetTile(randPos, ETile.Monster);

            newEnemy.name = newEnemy.name + i;
        }
    }

    private void ConnectEntrances()
    {
        entrancesPair = new List<(Entrance, Entrance)>();
        MakeEntrancePair();

        entrancesPair.Sort((pair1, pair2) =>
        {
            float dist1 = Vector2.Distance(pair1.Item1.pos, pair1.Item2.pos);
            float dist2 = Vector2.Distance(pair2.Item1.pos, pair2.Item2.pos);
            return dist1.CompareTo(dist2);
        });

        Union_Find(allEntrances, entrancesPair);
    }

    public void AddEntrance(Entrance newEntrance)
    {
        allEntrances.Add(newEntrance);
    }

    private void MakeEntrancePair()
    {
        for(int i=0; i<allEntrances.Count - 1; i++)
        {
            var a = allEntrances[i];

            for (int j=i; j<allEntrances.Count; j++)
            {
                var b = allEntrances[j];

                if (a.roomN != b.roomN)
                {
                    var pair = (a, b);
                    entrancesPair.Add(pair);
                }
            }
        }
    }

    private void Union_Find(List<Entrance> allEntrances, List<(Entrance, Entrance)> entrancesPair)
    {
        Dictionary<int, int> parent = new Dictionary<int, int>();

        foreach (var Entrance in allEntrances)
        {
            parent[Entrance.roomN] = Entrance.roomN;
        }

        foreach(var (a, b) in entrancesPair)
        {
            int roomA = Find(a.roomN, parent);
            int roomB = Find(b.roomN, parent);
         
            if(roomA != roomB)
            {
                CreateRoad(a, b);

                parent[roomB] = parent[roomA];
            }
            else
            {
                // .N퍼 확률로 싸이클 만들기?
            }
        }
    }

    private int Find(int roomN, Dictionary<int, int> parent)
    {
        if (roomN != parent[roomN])
        {
            parent[roomN] = Find(parent[roomN], parent);
        }

        return parent[roomN];
    }

    private void CreateRoad(Entrance a, Entrance b)
    {
        Vector2Int start = a.pos;
        Vector2Int end = b.pos;

        int index = Random.Range(0, 2);

        while (start != end)
        {
            index %= 2;

            Vector2Int dir = Vector2Int.zero;

            if (start[index] == end[index])
            {
                index++;

                continue;
            }
            else
            {
                dir[index] = (start[index] - end[index]) > 0 ? -1 : 1;
            }

            start += dir;

            if (map[start.y, start.x] != ETile.Empty)
                Instantiate(road, new Vector3(start.x, start.y, 0), Quaternion.identity, gameMap.transform);

            map[start.y, start.x] = ETile.Empty;
        }
    }
}