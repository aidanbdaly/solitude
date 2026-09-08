namespace Solitude.Simulator.Core.Model;
 
public sealed record EntityFeature
{
    public long EntityId { get; init; }
    public Feature Type { get; init; }
}
