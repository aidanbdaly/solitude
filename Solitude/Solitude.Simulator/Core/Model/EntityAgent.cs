namespace Solitude.Simulator.Core.Model;

public sealed record EntityAgent
{
    public long EntityId { get; init; }
    public required string Name { get; init; }
    public Agent Race { get; init; }
    public AgentDrive Drive { get; init; }
    public AgentStature Size { get; init; }
    public double Tiredness { get; init; }
    public double Hunger { get; init; }
}
