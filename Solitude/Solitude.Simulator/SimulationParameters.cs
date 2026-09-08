using Solitude.Simulator.Request;

namespace Solitude.Simulator;

public sealed record SimulationParameters
{
    public required string Path { get; init; }
    public required GenerateScenario Scenario { get; init; }
}