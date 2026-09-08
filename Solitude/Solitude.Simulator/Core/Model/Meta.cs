namespace Solitude.Simulator.Core.Model;

public sealed record Meta
{
    public int Singleton { get; init; }
    public int DataVersion { get; init; }
    public long Elapsed { get; init; }
    public required int Seed { get; init; }
    public int ActiveMapX { get; init; }
    public int ActiveMapY { get; init; }
}
