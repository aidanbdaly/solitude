



public class GameFactory(FeatureFactory feature, AgentFactory agent)
{
    private readonly FeatureFactory _feature = feature;

    private readonly AgentFactory _agent = agent;

    public GameState Create(GameParameters definition)
    {
        GameState game = new()
        {
            Map = new MapState(definition.Width, definition.Height),
            Time = new Time()
        };

        GenerateTerrain(game.Map, definition.TerrainGenerationParameters);
        GenerateColonists(game, definition.Colonists);

        return game;
    }

    private void GenerateTerrain(MapState map, GenerationParameters parameters)
    {
        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                var noise = PerlinNoise.Noise2D(
                    x * parameters.NoiseScale,
                    y * parameters.NoiseScale);

                var type = noise < parameters.WaterThreshold
                    ? TileType.Water
                    : noise < parameters.GrassThreshold ? TileType.Grass : TileType.Stone;

                map.SetTile(new(x, y), new(type));

                if (type == TileType.Stone)
                {
                    map.SetFeature(new(x, y), feature.Create(FeatureType.Rock));
                }

                if (type == TileType.Grass)
                {
                    map.SetFeature(new(x, y), feature.Create(FeatureType.Flora)); // Spread out
                }
            }
        }
    }

    private void GenerateColonists(GameState game, AgentParameters[] agentDefinitions)
    {
        foreach (var agentDefinition in agentDefinitions)
        {
            game.Map.SetAgent(new(1, 1), agent.Create(game.NextAgentId, agentDefinition)); // Place in unoccupied
        }

    }
}