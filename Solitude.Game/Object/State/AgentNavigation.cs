using System.Collections.Generic;
using Godot;
using Solitude.Domain.Game.Map;

public sealed class AgentNavigation
{
    public Queue<Vector2I> Path { get; } = new();
}
