using System;
using Godot;

namespace Solitude.Domain.Game;

public sealed class Clock
{
    public const float RealSecondsPerDay = 600f;
    public const float DefaultElapsedSeconds = RealSecondsPerDay * 0.3f;

    public float ElapsedSeconds { get; private set; }

    public Clock(float elapsedSeconds = DefaultElapsedSeconds)
    {
        if (!float.IsFinite(elapsedSeconds) || elapsedSeconds < 0f)
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
        ElapsedSeconds = elapsedSeconds;
    }

    public void Advance(float delta)
    {
        if (!float.IsFinite(delta) || delta < 0f)
            throw new ArgumentOutOfRangeException(nameof(delta));
        ElapsedSeconds += delta;
    }

    public float NormalizedDayTime => (ElapsedSeconds % RealSecondsPerDay) / RealSecondsPerDay;
    public int Day => (int)(ElapsedSeconds / RealSecondsPerDay) + 1;
    public int Hour => (int)(NormalizedDayTime * 24f);
    public int Minute => (int)(NormalizedDayTime * 24f * 60f) % 60;
    public string DisplayText => $"Day {Day}  {Hour:00}:{Minute:00}";
    public float LightLevel => Mathf.Clamp(
        0.18f + 0.82f * (0.5f + 0.5f * Mathf.Sin((NormalizedDayTime - 0.25f) * Mathf.Tau)),
        0.18f,
        1f);
}
