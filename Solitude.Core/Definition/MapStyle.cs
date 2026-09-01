public class MapStyle
{
    public required uint Width;
    public required uint Height;
    public required GenerationParameters Generation;

    public static MapStyle Default => new()
    {
        Width = 160,
        Height = 160,
        Generation = GenerationParameters.Default,
    };
}