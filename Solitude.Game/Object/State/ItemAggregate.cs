using System.Collections.Generic;

public sealed class ItemAggregate(List<ItemRequirement> required)
{
    public IReadOnlyList<ItemRequirement> Required { get; } = required;

    private Dictionary<ItemType, int> Contents = [];
}
