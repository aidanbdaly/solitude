using Godot;
using System;
using System.Linq;
using Solitude.Domain.Game;
using Solitude.Domain.Game.Agents;
using Solitude.Domain.Game.Map;
using Solitude.Domain.Game.Objects;

namespace Solitude.Domain.Game.Generation;

public sealed class WorldGenerator
{
    public State Generate(
        WorldGeneratorParameters parameters,
        int seed,
        AgentDefinition agentDefinition)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(agentDefinition);

        var grid = new Grid(parameters.Width, parameters.Height);
        var world = new World(grid);
        var random = new Random(seed);

        for (var y = 0; y < parameters.Height; y++)
        for (var x = 0; x < parameters.Width; x++)
        {
            var noise = UnrealPerlinNoise.Noise2D(
                x * parameters.NoiseScale,
                y * parameters.NoiseScale);
            var type = noise < parameters.WaterThreshold
                ? TileType.Water
                : noise < parameters.GrassThreshold ? TileType.Grass : TileType.Stone;
            var sameNorth = y > 0 && grid[new Vector2I(x, y - 1)].TileType == type;
            grid.SetCell(
                new Vector2I(x, y),
                type,
                sameNorth ? TileVariation.Connected : TileVariation.Isolated);
        }

        foreach (var pair in grid.Cells)
        {
            if (pair.Value.TileType == TileType.Stone)
                world.CreateObject(pair.Key, new RockObject());
            else if (pair.Value.TileType == TileType.Grass
                && random.NextDouble() < parameters.FloraDensity)
                world.CreateObject(pair.Key, new FloraObject());
        }

        var names = new[] { "Ada", "Mara", "Sol" };
        for (var index = 0; index < parameters.InitialAgentCount; index++)
        {
            var desired = new Vector2I(
                parameters.Width / 2 + (index - 1) * 2,
                parameters.Height / 2);
            var cell = FindNearestOpen(desired, world);
            var name = index < names.Length ? names[index] : $"Colonist {index + 1}";
            world.CreateAgent(cell, name, agentDefinition);
        }

        return new State(world, new Clock());
    }

    private static Vector2I FindNearestOpen(Vector2I desired, World world)
    {
        for (var radius = 0; radius < world.Grid.Width + world.Grid.Height; radius++)
        for (var offsetX = -radius; offsetX <= radius; offsetX++)
        {
            var offsetY = radius - Math.Abs(offsetX);
            foreach (var cell in new[]
            {
                desired + new Vector2I(offsetX, offsetY),
                desired + new Vector2I(offsetX, -offsetY)
            })
            {
                if (world.CanOccupy(cell) && !world.AgentsAt(cell).Any()) return cell;
            }
        }
        return Vector2I.Zero;
    }
}
