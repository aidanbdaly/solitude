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

    internal float IncreaseTiredness(float amount)
    {
        ValidateAmount(amount);

        var previous = Tiredness;
        Tiredness = Math.Min(1f, Tiredness + amount);
        return Tiredness - previous;
    }

    internal float DecreaseTiredness(float amount)
    {
        ValidateAmount(amount);

        var previous = Tiredness;
        Tiredness = Math.Max(0f, Tiredness - amount);
        return previous - Tiredness;
    }

    internal float IncreaseSatiation(float amount)
    {
        ValidateAmount(amount);

        var previous = Satiation;
        Satiation = Math.Min(1f, Satiation + amount);
        return Satiation - previous;
    }

    internal float DecreaseSatiation(float amount)
    {
        ValidateAmount(amount);

        var previous = Satiation;
        Satiation = Math.Max(0f, Satiation - amount);
        return previous - Satiation;
    }

    private static void ValidateAmount(float amount)
    {
        if (!float.IsFinite(amount) || amount <= 0f)
            throw new ArgumentOutOfRangeException(nameof(amount));
    }
}
