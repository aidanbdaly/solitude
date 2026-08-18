using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Solitude.Domain.Game.Map;

namespace Solitude.Domain.Game.Agents.Planning;

public sealed class PlanningNavigationMap
{
    private readonly ImmutableHashSet<Vector2I> _occupiableCells;

    public PlanningNavigationMap(IEnumerable<Vector2I> occupiableCells)
    {
        ArgumentNullException.ThrowIfNull(occupiableCells);
        _occupiableCells = occupiableCells.ToImmutableHashSet();
    }

    public bool TryFindPathCost(
        Vector2I start,
        Vector2I target,
        PathGoalMode goalMode,
        out Vector2I destination,
        out float cost)
    {
        destination = default;
        cost = 0f;
        if (goalMode == PathGoalMode.ExactCell && !_occupiableCells.Contains(target))
            return false;
        if (IsGoal(start, target, goalMode))
        {
            destination = start;
            return true;
        }

        var frontier = new PriorityQueue<Vector2I, float>();
        var costs = new Dictionary<Vector2I, float> { [start] = 0f };
        frontier.Enqueue(start, 0f);
        var visited = 0;

        while (frontier.TryDequeue(out var current, out _) && visited++ < 32768)
        {
            if (IsGoal(current, target, goalMode))
            {
                destination = current;
                cost = costs[current];
                return true;
            }

            foreach (var direction in GridMath.EightWayDirections)
            {
                var next = current + direction;
                if (!CanStep(current, next)) continue;
                var candidate = costs[current] + GridMath.StepCost(direction);
                if (costs.TryGetValue(next, out var known) && candidate >= known) continue;
                costs[next] = candidate;
                frontier.Enqueue(next, candidate);
            }
        }

        return false;
    }

    private bool CanStep(Vector2I from, Vector2I to)
    {
        var direction = to - from;
        if (!_occupiableCells.Contains(to)
            || Math.Abs(direction.X) > 1
            || Math.Abs(direction.Y) > 1
            || direction == Vector2I.Zero)
            return false;
        if (direction.X == 0 || direction.Y == 0) return true;
        return _occupiableCells.Contains(from + new Vector2I(direction.X, 0))
            && _occupiableCells.Contains(from + new Vector2I(0, direction.Y));
    }

    private static bool IsGoal(
        Vector2I cell,
        Vector2I target,
        PathGoalMode goalMode) =>
        goalMode == PathGoalMode.AdjacentToCell
            ? GridMath.IsAdjacent(cell, target)
            : cell == target;
}
