using UnityEngine;

public class Entrance
{
    public Vector2Int pos;
    public int roomN;

    public Entrance(Vector2 inPos, int inRoomN)
    {
        pos = Vector2Int.RoundToInt(inPos);
        roomN = inRoomN;
    }
}
