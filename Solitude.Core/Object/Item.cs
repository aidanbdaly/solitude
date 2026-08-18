public sealed class Item
{
    public required ItemId Id { get; init; }
    public required ItemType Type { get; init; }
    public int Count { get; internal set; }

    internal ItemStack Take(int requestedCount)
    {
        if (requestedCount <= 0)
            throw new System.ArgumentOutOfRangeException(nameof(requestedCount));
        var taken = System.Math.Min(requestedCount, Count);
        Count -= taken;
        return new ItemStack(Type, taken);
    }

    public bool IsDepleted => Count <= 0;
}