using System.Collections.Generic;
using Solitude.Domain.Game.Map;

public sealed class AgentNavigation
{
    public Queue<Coordinate> Path { get; } = new();
}
