using System;

public sealed class Time
{
    public const uint CycleLength = 600;
    public const uint CycleStart = 200;

    private uint Current { get; set; } = CycleStart;

    public void Set(uint time) => Current = time;
    public void Set(Func<uint, uint> setStageDelegate) => Current = setStageDelegate(Current);

    public uint Get() => Current;
}
