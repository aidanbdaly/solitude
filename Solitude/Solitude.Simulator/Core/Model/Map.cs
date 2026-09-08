namespace Solitude.Simulator.Core.Model;

public sealed record Map
{
    public int X { get; init; }
    public int Y { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
}
