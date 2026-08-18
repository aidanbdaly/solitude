using Godot;
using System.Collections.Generic;
using Solitude.Domain.Game.Map;

namespace Solitude.Domain.Game.Agents;

public enum AgentNavigationStatus
{
    Idle,
    Moving,
    Succeeded,
    Failed
}

public sealed class AgentNavigation
{
    public Queue<Vector2I> Path { get; } = new();
    public Vector2I Target { get; private set; }
    public PathGoalMode GoalMode { get; private set; }
    public AgentNavigationStatus Status { get; private set; }

    public void Begin(Vector2I target, PathGoalMode goalMode, IEnumerable<Vector2I> path)
    {
        Target = target;
        GoalMode = goalMode;
        Path.Clear();
        foreach (var cell in path) Path.Enqueue(cell);
        Status = Path.Count == 0 ? AgentNavigationStatus.Succeeded : AgentNavigationStatus.Moving;
    }

    internal void ReplacePath(IEnumerable<Vector2I> path)
    {
        Path.Clear();
        foreach (var cell in path) Path.Enqueue(cell);
        Status = Path.Count == 0 ? AgentNavigationStatus.Succeeded : AgentNavigationStatus.Moving;
    }

    internal void Succeed() => Status = AgentNavigationStatus.Succeeded;
    internal void Fail() => Status = AgentNavigationStatus.Failed;

    public void Reset()
    {
        Path.Clear();
        Target = default;
        GoalMode = default;
        Status = AgentNavigationStatus.Idle;
    }
}
