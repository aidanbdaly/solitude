namespace Solitude.Simulator.Core.Model;
 
public sealed record EntityInventoryItem
{
    public long EntityId { get; init; }
    public Item ItemType { get; init; }
    public int Quantity { get; init; }
}