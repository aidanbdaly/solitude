using System;

namespace Solitude.Domain.Game.Generation;

public sealed record WorldGeneratorParameters
{
    public int Width { get; }
    public int Height { get; }
    public int InitialAgentCount { get; }
    public float NoiseScale { get; }
    public float WaterThreshold { get; }
    public float GrassThreshold { get; }
    public float FloraDensity { get; }

    public static WorldGeneratorParameters Default { get; } = new(
        width: 160,
        height: 160,
        initialAgentCount: 3,
        noiseScale: 0.08f,
        waterThreshold: -0.25f,
        grassThreshold: 0.2f,
        floraDensity: 0.115f);

    public WorldGeneratorParameters(
        int width,
        int height,
        int initialAgentCount,
        float noiseScale,
        float waterThreshold,
        float grassThreshold,
        float floraDensity)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (initialAgentCount < 0) throw new ArgumentOutOfRangeException(nameof(initialAgentCount));
        if (!float.IsFinite(noiseScale) || noiseScale <= 0f)
            throw new ArgumentOutOfRangeException(nameof(noiseScale));
        if (!float.IsFinite(waterThreshold))
            throw new ArgumentOutOfRangeException(nameof(waterThreshold));
        if (!float.IsFinite(grassThreshold) || grassThreshold < waterThreshold)
            throw new ArgumentOutOfRangeException(nameof(grassThreshold));
        if (!float.IsFinite(floraDensity) || floraDensity < 0f || floraDensity > 1f)
            throw new ArgumentOutOfRangeException(nameof(floraDensity));

        Width = width;
        Height = height;
        InitialAgentCount = initialAgentCount;
        NoiseScale = noiseScale;
        WaterThreshold = waterThreshold;
        GrassThreshold = grassThreshold;
        FloraDensity = floraDensity;
    }
}
