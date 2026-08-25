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
 
}
