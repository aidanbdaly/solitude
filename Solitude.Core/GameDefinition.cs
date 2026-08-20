public class GameMapDefinition
{
    public required uint Width;
    public required uint Height;
    public required uint InitialAgentCount;
    public required float NoiseScale;
    public required float WaterThreshold;
    public required float GrassThreshold;
    public required float FloraDensity;

    public static GameMapDefinition Default => new()
    {
        Width = 160,
        Height = 160,
        InitialAgentCount = 3,
        NoiseScale = 0.08f,
        WaterThreshold = -0.25f,
        GrassThreshold = 0.2f,
        FloraDensity = 0.115f
    };
}

public class GameTimeDefinition
{

}

public class GameDefinition
{
    public GameMapDefinition Map;
    public GameTimeDefinition Time;
}