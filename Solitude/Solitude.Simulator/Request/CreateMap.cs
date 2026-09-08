using Solitude.Simulator.Core.Model;

namespace Solitude.Simulator.Request;

public sealed record CreateMap
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required IReadOnlyList<Tile> Tiles { get; init; }
}
