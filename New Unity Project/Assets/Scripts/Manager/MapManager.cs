using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MapManager : MonoBehaviour
{
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
    private int roomMinN = 4;
    private int roomN;
    
    private GameObject gameMap;

    const int maxRoomMakeAttempts = 100;

    private void Awake()
    {
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
        Vector2Int clearPos = Vector2Int.FloorToInt(pos);

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
        Vector2Int clearPos = Vector2Int.FloorToInt(pos);

        int rows = map.GetLength(0);
        int cols = map.GetLength(1);

        if (map == null || clearPos.y < 0 || clearPos.y >= rows || clearPos.x < 0 || clearPos.x >= cols)
        {
            Debug.LogError("타일 청소 실패");
            return;
        }

        map[clearPos.y, clearPos.x] = newTileCondition;
    }

    public ETile GetTileCondition(Vector2 pos)
    {
        return map[(int)pos.y, (int)pos.x];
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
            }
        }
    }

    private void CreatePlayer()
    {
        int startRoomN = Random.Range(0, roomN);

        Vector2 startPos = rooms[startRoomN].GetRandomTilePos(map);
        map[(int)startPos.y, (int)startPos.x] = ETile.Player;

        Instantiate<Player>(playerPrefab, startPos, Quaternion.identity, gameMap.transform);
    }
}
