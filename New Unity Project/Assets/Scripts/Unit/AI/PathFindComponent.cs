using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathFindComponent : MonoBehaviour
{
    class Node
    {
        public Vector2Int pos;
        public Node parent = null;
        public int hCost, gCost;
        public int fCost => hCost + gCost;

        public Node(Vector2Int inPos) { this.pos = inPos; }
    }

    public Stack<Vector2Int> paths { get; private set; }

    private void Start()
    {
        paths = new Stack<Vector2Int>();
    }

    private void Update()
    {
        var prePath = transform.position;
        foreach (var path in paths)
        {
            Debug.DrawLine(prePath, (Vector2)path);
            prePath = (Vector2)path;
        }
    }

    public void PathFind(Vector2Int startPos, Vector2Int goal)
    {
        paths = new Stack<Vector2Int>();

        List<Node> open = new List<Node>();
        HashSet<Vector2Int> close = new HashSet<Vector2Int>();

        Node startNode = new Node(startPos) { gCost = 0, hCost = Heuristic(startPos, goal) };
        open.Add(startNode);

        while (open.Count > 0)
        {
            open.Sort((a, b) => a.fCost.CompareTo(b.fCost));
            Node node = open[0];

            if (node.pos == goal)
            {
                Backtracking(node);
                return;
            }

            open.RemoveAt(0);
            close.Add(node.pos);

            foreach (var dir in Enemy.dirs)
            {
                Vector2Int end = node.pos + Vector2Int.RoundToInt(dir);
                if (!CanMovePos(node.pos, dir) || close.Contains(end))
                    continue;

                int moveCost = node.gCost + ((dir.x != 0 && dir.y != 0) ? 14 : 10);
                Node existing = open.FirstOrDefault(n => n.pos == end);

                if (existing != null)
                {
                    if (moveCost < existing.gCost)
                    {
                        existing.gCost = moveCost;
                        existing.parent = node;
                    }
                }
                else
                {
                    Node newNode = new Node(end)
                    {
                        gCost = moveCost,
                        hCost = Heuristic(end, goal),
                        parent = node
                    };

                    open.Add(newNode);
                }
            }
        }
    }

    private void Backtracking(Node node)
    {
        paths = new Stack<Vector2Int>();

        while(node?.parent != null)
        {
            paths.Push(node.pos);
            node = node.parent;
        }
    }

    private int Heuristic(Vector2 start, Vector2 end) // 대각선 방향을 고려하여 Chebyshev 사용
    {
        int x = Mathf.RoundToInt(Mathf.Abs(start.x - end.x));
        int y = Mathf.RoundToInt(Mathf.Abs(start.y - end.y));

        return 10 * Mathf.Max(x, y);
    }

    private bool CanMovePos(Vector2Int startPos, Vector2Int dir)
    {
        Vector2Int targetPos = startPos + dir;

        if (!MapManager.Instance.IsInMapBounds(targetPos) || !MapManager.Instance.CanCrossWalk(startPos, dir))
            return false;

        ETile targetTileCondition = MapManager.Instance.GetTileType(targetPos);
        if (targetTileCondition != ETile.Empty && targetTileCondition != ETile.Player)
            return false;

        return true;
    }

    public bool IsPathStillValid()
    {
        if (paths.Count == 0)
            return false;

        Stack<Vector2Int> newPath = new Stack<Vector2Int>();

        while(paths.Count > 0)
        {
            Vector2Int node = paths.Pop();

            ETile tile = MapManager.Instance.GetTileType(node);
            
            if(tile == ETile.Monster)
            {
                return false;
            }
            else //벽이 갑자기 생기는 경우는 없음
            {
                //newPath.Push(node);

                if(tile == ETile.Player)
                {
                    break;
                }
            }
        }

        paths = newPath;

        return true;
    }
}
