using System;

namespace Solitude.Domain.Game.Items;

public readonly record struct ItemRequirement
{
    public ItemType Type { get; }
    public int Count { get; }

    public ItemRequirement(ItemType type, int count)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
        Type = type;
        Count = count;
    }
}
