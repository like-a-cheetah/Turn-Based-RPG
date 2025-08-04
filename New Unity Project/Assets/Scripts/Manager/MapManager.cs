using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    private ETile[,] map;

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
    }

    void Start()
    {
        Enemy.onDeath += TileClear;

        GenerateRooms();

        CreatePlayer();

        GeneratesEnemies();
        
        MovingObject.onMoveUnit += (MovingObject obj, Vector2 moveDir) =>
        {
            Vector2 startPos = obj.transform.position;
            TileClear(startPos);
            SetTile(startPos + moveDir, obj.tileType);
        };
        //Player.onMoveStart += (Vector2 TargetPos) => { SetTile(TargetPos, ETile.Player); };
    }

    void Update()
    {
        //DebugMapSpace();
    }

    public void TileClear(Enemy enemy)
    {
        TileClear(enemy.transform.position);
    }

    public void TileClear(Vector2 pos)
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

    public void SetTile(Vector2 pos, ETile newTileCondition)
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

    public ETile GetTileType(Vector2 pos)
    {
        return map[(int)pos.y, (int)pos.x];
    }

    public bool CanCrossWalk(Vector2 startPos, Vector2 dir)
    {
        if ((dir.x != 0 || dir.y != 0))
        {
            Vector2 tmpDir, aroundPos;

            tmpDir = dir;
            tmpDir.x = 0;

            aroundPos = startPos + tmpDir;
            if (GetTileType(aroundPos) == ETile.Wall)
                return false;

            tmpDir = dir;
            tmpDir.y = 0;

            aroundPos = startPos + tmpDir;
            if (GetTileType(aroundPos) == ETile.Wall)
                return false;
        }

        return true;
    }

    private void GenerateRooms()
    {
        for (int i = 0; i < roomN; i++)
        {
            Room newRoom = InstantiateRandomRoom();
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

    private Room InstantiateRandomRoom()
    {
        int roomWidth = Random.Range(roomMinLine, roomMaxLine);
        int roomHeight = Random.Range(roomMinLine, roomMaxLine);

        Room newRoom = Instantiate<Room>(room, transform.position, Quaternion.identity, gameMap.transform);
        newRoom.width = roomWidth;
        newRoom.height = roomHeight;

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
                if (map[y, x] == ETile.Wall)
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

        Vector2 startPos = rooms[startRoomN].GetRandomTilePos(map);

        Instantiate<Player>(playerPrefab, startPos, Quaternion.identity, gameMap.transform);
    }

    private void GeneratesEnemies()
    {
        int enemyN = enemyMaxN[floor];

        for(int i=0; i<enemyN; i++)
        {
            Vector2 randPos;
            do
            {
                int randRoomN = Random.Range(0, roomN);

                randPos = rooms[randRoomN].GetRandomTilePos(map);
            }
            while (randPos == new Vector2(-1, -1));

            Instantiate<Enemy>(enemiesPF[0], randPos, Quaternion.identity, gameMap.transform);
        }
    }
}
