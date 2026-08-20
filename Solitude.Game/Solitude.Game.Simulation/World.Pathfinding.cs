using Godot;
using System;
using System.Collections.Generic;
using Solitude.Domain.Game.Map;

namespace Solitude.Domain.Game;

public sealed partial class World
{
    public bool CanOccupy(Vector2I cell) =>
        Grid.IsTerrainTraversable(cell)
        && ConstructionSiteAt(cell) is null
        && (ObjectAt(cell)?.IsWalkable ?? true);

    public bool CanStep(Vector2I from, Vector2I to)
    {
        var direction = to - from;
        if (!CanOccupy(to)
            || Math.Abs(direction.X) > 1
            || Math.Abs(direction.Y) > 1
            || direction == Vector2I.Zero)
            return false;
        if (direction.X == 0 || direction.Y == 0) return true;
        return CanOccupy(from + new Vector2I(direction.X, 0))
            && CanOccupy(from + new Vector2I(0, direction.Y));
    }

    public bool TryFindPath(
        Vector2I start,
        Vector2I target,
        PathGoalMode goalMode,
        out List<Vector2I> path)
    {
        path = new List<Vector2I>();
        if (goalMode == PathGoalMode.ExactCell && !CanOccupy(target)) return false;
        if (IsPathGoal(start, target, goalMode))
        {
            path.Add(start);
            return true;
        }

        var frontier = new PriorityQueue<Vector2I, float>();
        var cameFrom = new Dictionary<Vector2I, Vector2I>();
        var costs = new Dictionary<Vector2I, float> { [start] = 0f };
        frontier.Enqueue(start, 0f);
        var visited = 0;

        while (frontier.TryDequeue(out var current, out _) && visited++ < 32768)
        {
            if (IsPathGoal(current, target, goalMode))
            {
                for (var step = current;; step = cameFrom[step])
                {
                    path.Add(step);
                    if (step == start) break;
                }
                path.Reverse();
                return true;
            }

            foreach (var direction in GridMath.EightWayDirections)
            {
                var next = current + direction;
                if (!CanStep(current, next)) continue;
                var candidate = costs[current] + GridMath.StepCost(direction);
                if (costs.TryGetValue(next, out var known) && candidate >= known) continue;
                costs[next] = candidate;
                cameFrom[next] = current;
                frontier.Enqueue(next, candidate + GridMath.Octile(next, target));
            }
        }
        return false;
    }

    private static bool IsPathGoal(Vector2I cell, Vector2I target, PathGoalMode mode) =>
        mode == PathGoalMode.AdjacentToCell ? GridMath.IsAdjacent(cell, target) : cell == target;
}
