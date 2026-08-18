using System;
using System.Collections.Generic;
using System.Linq;

public sealed class Inventory
{
    private readonly Dictionary<ItemType, int> _counts = new();

    public int Capacity { get; }
    public int TotalCount => _counts.Values.Sum();
    public int AvailableCapacity => Math.Max(0, Capacity - TotalCount);

    public Inventory(int capacity = int.MaxValue)
    {
        if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        Capacity = capacity;
    }

    public int GetCount(ItemType type) => _counts.GetValueOrDefault(type);

    public int Add(ItemType type, int requestedCount)
    {
        if (requestedCount < 0) throw new ArgumentOutOfRangeException(nameof(requestedCount));
        var count = Math.Min(requestedCount, AvailableCapacity);
        if (count > 0) _counts[type] = GetCount(type) + count;
        return count;
    }

    public int Remove(ItemType type, int requestedCount)
    {
        if (requestedCount < 0) throw new ArgumentOutOfRangeException(nameof(requestedCount));
        var count = Math.Min(requestedCount, GetCount(type));
        if (count <= 0) return 0;
        var remaining = GetCount(type) - count;
        if (remaining == 0) _counts.Remove(type); else _counts[type] = remaining;
        return count;
    }

    public int TransferTo(Inventory destination, ItemType type, int requestedCount)
    {
        ArgumentNullException.ThrowIfNull(destination);
        if (requestedCount < 0) throw new ArgumentOutOfRangeException(nameof(requestedCount));
        var count = Math.Min(Math.Min(requestedCount, GetCount(type)), destination.AvailableCapacity);
        Remove(type, count);
        destination.Add(type, count);
        return count;
    }

    public IReadOnlyList<ItemStack> GetStacks() => _counts.Select(pair => new ItemStack(pair.Key, pair.Value)).ToArray();
}
