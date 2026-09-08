namespace Solitude.Simulator.Core.Model;

public sealed record EntityItem
{
    public long EntityId { get; init; }
    public Item Type { get; init; }
    public int Quantity { get; init; }
}
