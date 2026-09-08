// These are biome characteristics
using Solitude.Simulator.Core.Model;

public sealed record BiomeFingerprint
{
    public required float NoiseScale { get; init; }
    public required float FloraDensity { get; init; }
    public required TilePallete Pallete { get; init; }

    public Tile GetTile(int x, int y)
    {
        var noise = PerlinNoise.Noise2DByte(
                    x * NoiseScale,
                    y * NoiseScale);

        var type = Pallete[noise];

        if (type == Tile.Stone)
        {
            map.SetFeature(new(Feature.Rock), new(x, y));
        }

        if (type == Tile.Grass && random.NextSingle() < request.FloraDensity)
        {
            map.SetFeature(new(Feature.Flora), new(x, y));
        }
    }
}
