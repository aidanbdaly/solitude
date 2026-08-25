public sealed class Item
{
    public required long Id { get; init; }
    public required ItemType Type { get; init; }
    public int Count { get; internal set; }
}