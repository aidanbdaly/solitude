namespace Solitude.Simulator.Core.Model;

public sealed record EntityPosition
{
    public long EntityId { get; init; }
    public int MapX { get; init; }
    public int MapY { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
}