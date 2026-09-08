namespace Solitude.Simulator.Core.Model;

public sealed record MapTile
{
    public int MapX { get; init; }
    public int MapY { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public Tile Type { get; init; }
}
