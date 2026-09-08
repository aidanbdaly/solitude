using Solitude.Simulator.Core.Model;

namespace Solitude.Simulator.Request;

public sealed record GenerateScenario
{
    public required int MapX { get; init; }
    public required int MapY { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required Biome Biome { get; init; }
    public required IReadOnlyList<EntityAgent> AgentSet;
}