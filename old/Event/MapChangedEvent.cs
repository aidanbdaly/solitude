public sealed class MapChangedEvent : EventArgs
{
    public required Map NewMap { get; init; }
}
