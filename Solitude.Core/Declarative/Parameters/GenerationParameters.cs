public class GenerationParameters
{
    public required float NoiseScale;
    public required float WaterThreshold;
    public required float GrassThreshold;
    public required float FloraDensity;

    public static GenerationParameters Default => new()
    {
        NoiseScale = 0.08f,
        WaterThreshold = -0.25f,
        GrassThreshold = 0.2f,
        FloraDensity = 0.115f
    };
}