using System.Collections.Generic;

public sealed class ItemAggregate(List<ItemRequirement> required) : Dictionary<ItemType, int>
{
    public IReadOnlyList<ItemRequirement> Required { get; } = required;
}
