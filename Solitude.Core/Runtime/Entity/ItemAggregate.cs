using System.Collections.Generic;

public sealed class ItemAggregate(List<ItemRequirement> required)
{
    public IReadOnlyList<ItemRequirement> Required { get; } = required;
    public Inventory Current = new();
}
