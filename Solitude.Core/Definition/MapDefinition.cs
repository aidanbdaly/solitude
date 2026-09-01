public sealed record MapDefinition(
    uint Width,
    uint Height,
    int Seed,
    float NoiseScale,
    float FloraDensity,
    TerrainPalette Pallete
)
{
    public static MapDefinition Default()
    {
        var pallete = new TerrainPalette();

        pallete[255] = TileType.Stone;
        pallete[50] = TileType.Grass;

        return new(160, 160, 1, 0.08f, 0.115f, pallete);
    }
}

public sealed record TerrainPalette
{
    private readonly TileType[] _band = new TileType[byte.MaxValue + 1];

    public TileType this[byte elevation]
    {
        get => _band[elevation];
        set
        {
            for (int i = elevation; i >= 0 && _band[i] != value; i--)
            {
                _band[i] = value;
            }
        }
    }
}