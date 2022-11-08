using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PathManager
{
    public static PathManager main = null;
    public static void Init(PathManager finder)
    {
        main = finder;
        finder.useWeights = true;
    }

    public float cellSize = 1;

    public PathManager(float cellSize)
    {
        this.cellSize = cellSize;
    }

    public bool useWeights = false;

    public Vector2Int[] RequestPath(Vector2Int start, Vector2Int end, Func<Vector2Int, bool> IsValidMove = null)
    {
        if (IsValidMove == null)
        {
            IsValidMove = IsValidMovePosition;
        }
        if (!IsValidMove(start) || !IsValidMove(end))
        {
            return null;
        }

        PathFinder path;
        if (useWeights)
        {
            path = new PathFinder(IsValidMove, MovePositionWeight);
        }
        else
        {
            path = new PathFinder(IsValidMove);
        }

        path.Start = start;
        path.End = end;
        return path.FindPath();
    }

    public Task<Vector2Int[]> RequestPathAsync(Vector2Int start, Vector2Int end, Func<Vector2Int, bool> IsValidMove = null)
    {
        if (IsValidMove == null)
        {
            IsValidMove = IsValidMovePosition;
        }
        PathFinder path;
        if (useWeights)
        {
            path = new PathFinder(IsValidMove, MovePositionWeight);
        }
        else
        {
            path = new PathFinder(IsValidMove);
        }

        Task<Vector2Int[]> task = path.FindPathAsync(start, end);

        return task;
    }

    public bool IsValidMovePosition(Vector2Int position)
    {
        int y = MapController.main.mapData.width;
        int x = MapController.main.mapData.length;

        if (position.x >= x || position.x < 0 || position.y >= y || position.y < 0) return false;

        if (MapController.main.mapData.tiles[MapController.main.mapData.GetIndex(position.y, position.x)].isPassable)
        {
            return true;
        }
        return false;
    }

    public float MovePositionWeight(Vector2Int position)
    {
        int y = MapController.main.mapData.width;
        int x = MapController.main.mapData.length;

        if (position.x >= x || position.x < 0 || position.y >= y || position.y < 0) return 0;

        if (!MapController.main.mapData.tiles[MapController.main.mapData.GetIndex(position.y, position.x)].isPassable)
        {
            return 0;
        }
        return MapController.main.mapData.tiles[MapController.main.mapData.GetIndex(position.y, position.x)].weight;
    }
}
