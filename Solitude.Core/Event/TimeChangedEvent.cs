public sealed class TimeChangedEvent : EventArgs
{
    public required uint Time { get; init; }
};
