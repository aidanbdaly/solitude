using System;
using Godot;



public sealed class Time
{
    public const uint CycleLength = 600;
    public const uint CycleStart = 200;

    public uint Current { get; private set; } = CycleStart;

    public void Set(uint time) => Current = time;
    public void Set(Func<uint, uint> setStageDelegate) => Current = setStageDelegate(Current);
}

