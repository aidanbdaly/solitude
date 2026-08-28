using Godot;
using Xunit;

public sealed class WorldTests
{
    private static readonly AgentDefinition AgentDefinition = global::AgentDefinition.Colonist;

    [Fact]
    public void CreateAgent_AllocatesStableIdsAndPlacesAgents()
    {
        var world = CreateWorld(new(2, 1));

        var firstId = world.CreateAgent(AgentDefinition, new(Vector2I.Zero, Vector2I.Zero));
        var secondId = world.CreateAgent(AgentDefinition, new(Vector2I.Zero, Vector2I.Right));

        Assert.Equal(0, firstId);
        Assert.Equal(1, secondId);
        Assert.Equal(firstId, world.GetMap(Vector2I.Zero).Agent.Get(Vector2I.Zero)?.Id);
        Assert.Equal(secondId, world.GetMap(Vector2I.Zero).Agent.Get(Vector2I.Right)?.Id);
    }

    [Fact]
    public void SetAgentAddress_MovesAgentWithinMap()
    {
        var world = CreateWorld(new(2, 1));
        var id = world.CreateAgent(AgentDefinition, new(Vector2I.Zero, Vector2I.Zero));

        world.MoveAgent(id, new(Vector2I.Zero, Vector2I.Right));

        var map = world.GetMap(Vector2I.Zero);
        Assert.Null(map.Agent.Get(Vector2I.Zero));
        Assert.Equal(id, map.Agent.Get(Vector2I.Right)?.Id);
    }

    [Fact]
    public void SetAgentAddress_MovesAgentBetweenMaps()
    {
        var world = CreateWorld(new(1, 1), new Vector2I(1, 0));
        var id = world.CreateAgent(AgentDefinition, new(Vector2I.Zero, Vector2I.Zero));

        world.MoveAgent(id, new(new(1, 0), Vector2I.Zero));

        Assert.Null(world.GetMap(Vector2I.Zero).Agent.Get(Vector2I.Zero));
        Assert.Equal(id, world.GetMap(new(1, 0)).Agent.Get(Vector2I.Zero)?.Id);
    }

    [Fact]
    public void SetAgentAddress_OccupiedDestinationPreservesOriginalPlacement()
    {
        var world = CreateWorld(new(2, 1));
        var firstId = world.CreateAgent(AgentDefinition, new(Vector2I.Zero, Vector2I.Zero));
        world.CreateAgent(AgentDefinition, new(Vector2I.Zero, Vector2I.Right));

        Assert.Throws<InvalidOperationException>(() =>
            world.MoveAgent(firstId, new(Vector2I.Zero, Vector2I.Right)));

        Assert.Equal(firstId, world.GetMap(Vector2I.Zero).Agent.Get(Vector2I.Zero)?.Id);
    }

    [Fact]
    public void SetAgentAddress_InvalidDestinationPreservesOriginalPlacement()
    {
        var world = new World(2, 1);
        world.CreateMap(CreateMapStyle(new(1, 1)), Vector2I.Zero);
        var id = world.CreateAgent(AgentDefinition, new(Vector2I.Zero, Vector2I.Zero));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            world.MoveAgent(id, new(Vector2I.Zero, Vector2I.Right)));
        Assert.Throws<InvalidOperationException>(() =>
            world.MoveAgent(id, new(new(1, 0), Vector2I.Zero)));

        Assert.Equal(id, world.GetMap(Vector2I.Zero).Agent.Get(Vector2I.Zero)?.Id);
    }

    [Fact]
    public void SetAgentAddress_CurrentAddressIsNoOpAndUnknownIdIsRejected()
    {
        var world = CreateWorld(new(1, 1));
        var address = new WorldAddress(Vector2I.Zero, Vector2I.Zero);
        var id = world.CreateAgent(AgentDefinition, address);

        world.MoveAgent(id, address);

        Assert.Equal(id, world.GetMap(Vector2I.Zero).Agent.Get(Vector2I.Zero)?.Id);
        Assert.Throws<InvalidOperationException>(() => world.MoveAgent(999, address));
    }

    [Fact]
    public void AgentIdCanExceedTheContainingMapsCellCount()
    {
        var coordinates = Enumerable.Range(0, 5).Select(x => new Vector2I(x, 0)).ToArray();
        var world = CreateWorld(new(1, 1), coordinates[1..]);

        long lastId = -1;
        foreach (var coordinate in coordinates)
        {
            lastId = world.CreateAgent(AgentDefinition, new(coordinate, Vector2I.Zero));
        }

        Assert.Equal(4, lastId);
        Assert.Equal(lastId, world.GetMap(coordinates[^1]).Agent.Get(Vector2I.Zero)?.Id);
    }

    private static World CreateWorld(Vector2I mapSize, params Vector2I[] additionalMapCoordinates)
    {
        var world = new World((uint)(additionalMapCoordinates.Length + 1), 1);
        var style = CreateMapStyle(mapSize);

        world.CreateMap(style, Vector2I.Zero);
        foreach (var coordinate in additionalMapCoordinates)
        {
            world.CreateMap(style, coordinate);
        }

        return world;
    }

    private static MapStyle CreateMapStyle(Vector2I mapSize)
        => new()
        {
            Width = (uint)mapSize.X,
            Height = (uint)mapSize.Y,
            Generation = new()
            {
                NoiseScale = 0f,
                WaterThreshold = float.MaxValue,
                GrassThreshold = float.MaxValue,
                FloraDensity = 0f
            }
        };
}
