using System.IO;
using System.Text.Json;
using Solitude.Persistence;
using Xunit;

public sealed class SnapshotTests
{
    [Fact]
    public void GameSnapshot_JsonRoundTripRestoresAllState()
    {
        var original = CreateCompleteSnapshot();
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<GameSnapshot>(json);

        Assert.NotNull(deserialized);

        var restored = deserialized.ToGame();
        var actual = restored.ToSnapshot();

        Assert.Equal(
            JsonSerializer.Serialize(original),
            JsonSerializer.Serialize(actual));
        Assert.Equal(7, restored.GetActiveMap().Agent.Get(new(0, 0))?.Id);
        Assert.Equal(9, restored.GetActiveMap().Item.Get(new(1, 0))?.Id);
    }

    [Fact]
    public void GameRestore_RejectsUnsupportedVersion()
    {
        var snapshot = CreateEmptySnapshot() with { Version = GameSnapshot.CurrentVersion + 1 };

        Assert.Throws<InvalidDataException>(() => snapshot.ToGame());
    }

    [Fact]
    public void MapRestore_RejectsIncorrectTileCount()
    {
        var snapshot = CreateEmptyMap() with { Tiles = [] };

        Assert.Throws<InvalidDataException>(() => snapshot.ToMap());
    }

    [Fact]
    public void WorldRestore_RejectsUnknownPlacementId()
    {
        var map = CreateEmptyMap() with
        {
            Agents = [new(new(0, 0), 42)]
        };
        var snapshot = new WorldSnapshot(1, 1, 0, 0, [], [], [map]);

        Assert.Throws<InvalidDataException>(() => snapshot.ToWorld());
    }

    [Fact]
    public void WorldRestore_RejectsInvalidNextIds()
    {
        var snapshot = CreateEmptySnapshot().World with { NextAgentId = -1 };

        Assert.Throws<InvalidDataException>(() => snapshot.ToWorld());
    }

    private static GameSnapshot CreateCompleteSnapshot()
    {
        var firstMap = new MapSnapshot(
            new(0, 0),
            4,
            2,
            321,
            [
                TileType.Grass, TileType.Water, TileType.Stone, TileType.Grass,
                TileType.Water, TileType.Grass, TileType.Stone, TileType.Water
            ],
            [new(new(0, 0), FeatureType.Flora)],
            [new(new(1, 0), 12, 5)],
            [new(new(0, 1), 2)],
            [new(
                new(2, 0),
                [new(ItemType.Wood, 4), new(ItemType.Stone, 2)],
                [new(ItemType.Wood, 3)])],
            [new(new(3, 0), 3)]);

        var secondMap = new MapSnapshot(
            new(1, 0),
            2,
            2,
            654,
            [TileType.Stone, TileType.Grass, TileType.Water, TileType.Grass],
            [],
            [],
            [new(new(1, 0), 9)],
            [],
            [new(new(0, 0), 7)]);

        var agents = new AgentSnapshot[]
        {
            new(
                3,
                AgentDefinition.Colonist,
                new() { Tiredness = 0.25f, Hunger = 0.5f },
                [new(ItemType.Wood, 2)]),
            new(
                7,
                new("Scout", AgentType.Human, AgentDrive.Acceptable, AgentStature.HugeHugo),
                new() { Tiredness = 0.75f, Hunger = 0.1f },
                [new(ItemType.Stone, 1)])
        };

        var items = new ItemSnapshot[]
        {
            new(2, ItemType.Wood, 8),
            new(9, ItemType.Stone, 3)
        };

        return new(
            GameSnapshot.CurrentVersion,
            new(2, 1, 8, 10, agents, items, [firstMap, secondMap]),
            new(1, 0));
    }

    private static GameSnapshot CreateEmptySnapshot()
        => new(
            GameSnapshot.CurrentVersion,
            new(1, 1, 0, 0, [], [], [CreateEmptyMap()]),
            null);

    private static MapSnapshot CreateEmptyMap()
        => new(
            new(0, 0),
            1,
            1,
            0,
            [TileType.Grass],
            [],
            [],
            [],
            [],
            []);
}
