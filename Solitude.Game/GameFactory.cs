public static class GameFactory
{
    public static GameState ToState(this GameDefinition definition)
    {
        var mapDefintion = definition.Map;

        var width = mapDefintion.Width;
        var height = mapDefintion.Height;

        var map = new Map(width, height);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var noise = UnrealPerlinNoise.Noise2D(
                    x * mapDefintion.NoiseScale,
                    y * mapDefintion.NoiseScale);

                var type = noise < mapDefintion.WaterThreshold
                    ? TileType.Water
                    : noise < mapDefintion.GrassThreshold ? TileType.Grass : TileType.Stone;

                map.SetTile(x, y, type);
            }
        }

        var time = new Time();

        return new()
        {
            Map = map,
            Time = time
        };
    }
}