using System;

namespace Solitude.Domain.Game.Agents;

public sealed class AgentNeeds
{
    public const float DefaultTiredness = 0.2f;
    public const float DefaultSatiation = 0.8f;

    public float Tiredness { get; private set; }
    public float Satiation { get; private set; }

    public AgentNeeds(
        float tiredness = DefaultTiredness,
        float satiation = DefaultSatiation)
    {
        if (!float.IsFinite(tiredness) || tiredness < 0f || tiredness > 1f)
            throw new ArgumentOutOfRangeException(nameof(tiredness));
        if (!float.IsFinite(satiation) || satiation < 0f || satiation > 1f)
            throw new ArgumentOutOfRangeException(nameof(satiation));

        Tiredness = tiredness;
        Satiation = satiation;
    }

    internal void Advance(float tirednessIncrease, float satiationDecrease)
    {
        if (!float.IsFinite(tirednessIncrease) || tirednessIncrease < 0f)
            throw new ArgumentOutOfRangeException(nameof(tirednessIncrease));
        if (!float.IsFinite(satiationDecrease) || satiationDecrease < 0f)
            throw new ArgumentOutOfRangeException(nameof(satiationDecrease));

        Tiredness = Math.Min(1f, Tiredness + tirednessIncrease);
        Satiation = Math.Max(0f, Satiation - satiationDecrease);
    }
}
