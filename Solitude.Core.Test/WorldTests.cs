using Xunit;

public sealed class WorldTests
{
    private static readonly AgentDefinition AgentDefinition = global::AgentDefinition.Colonist;

    [Fact]
    public void CreateAgent_AllocatesStableIdsAndPlacesAgents()
    {
        var world = CreateWorld(new(2, 1));

        var firstId = world.CreateAgent(AgentDefinition, new(Coordinate.Zero, Coordinate.Zero));
        var secondId = world.CreateAgent(AgentDefinition, new(Coordinate.Zero, Coordinate.Right));

        Assert.Equal(0, firstId);
        Assert.Equal(1, secondId);
        Assert.Equal(firstId, world.GetMap(Coordinate.Zero).Agent.Get(Coordinate.Zero)?.Id);
        Assert.Equal(secondId, world.GetMap(Coordinate.Zero).Agent.Get(Coordinate.Right)?.Id);
    }

    [Fact]
    public void MoveAgent_MovesAgentWithinMap()
    {
        var world = CreateWorld(new(2, 1));
        var id = world.CreateAgent(AgentDefinition, new(Coordinate.Zero, Coordinate.Zero));

        world.MoveAgent(id, new(Coordinate.Zero, Coordinate.Right));

        var map = world.GetMap(Coordinate.Zero);
        Assert.Null(map.Agent.Get(Coordinate.Zero));
        Assert.Equal(id, map.Agent.Get(Coordinate.Right)?.Id);
    }

    [Fact]
    public void MoveAgent_MovesAgentBetweenMaps()
    {
        var world = CreateWorld(new(1, 1), new Coordinate(1, 0));
        var id = world.CreateAgent(AgentDefinition, new(Coordinate.Zero, Coordinate.Zero));

        world.MoveAgent(id, new(new(1, 0), Coordinate.Zero));

        Assert.Null(world.GetMap(Coordinate.Zero).Agent.Get(Coordinate.Zero));
        Assert.Equal(id, world.GetMap(new(1, 0)).Agent.Get(Coordinate.Zero)?.Id);
    }

    [Fact]
    public void MoveAgent_OccupiedDestinationThrows()
    {
        var world = CreateWorld(new(2, 1));
        var agentId = world.CreateAgent(AgentDefinition, new(Coordinate.Zero, Coordinate.Zero));
        world.CreateAgent(AgentDefinition, new(Coordinate.Zero, Coordinate.Right));

        Assert.Throws<InvalidOperationException>(() =>
            world.MoveAgent(agentId, new(Coordinate.Zero, Coordinate.Right)));
    }

    [Fact]
    public void MoveAgent_InvalidDestinationThrows()
    {
        var outOfBoundsWorld = CreateWorld(new(1, 1));
        var outOfBoundsAgentId = outOfBoundsWorld.CreateAgent(
            AgentDefinition,
            new(Coordinate.Zero, Coordinate.Zero));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            outOfBoundsWorld.MoveAgent(
                outOfBoundsAgentId,
                new(Coordinate.Zero, Coordinate.Right)));

        var missingMapWorld = new World(2, 1);
        missingMapWorld.CreateMap(CreateMapStyle(new(1, 1)), Coordinate.Zero);
        var missingMapAgentId = missingMapWorld.CreateAgent(
            AgentDefinition,
            new(Coordinate.Zero, Coordinate.Zero));

        Assert.Throws<InvalidOperationException>(() =>
            missingMapWorld.MoveAgent(
                missingMapAgentId,
                new(new(1, 0), Coordinate.Zero)));
    }

    [Fact]
    public void MoveAgent_CurrentAddressIsNoOpAndUnknownIdIsRejected()
    {
        var world = CreateWorld(new(1, 1));
        var address = new WorldAddress(Coordinate.Zero, Coordinate.Zero);
        var id = world.CreateAgent(AgentDefinition, address);

        world.MoveAgent(id, address);

        Assert.Equal(id, world.GetMap(Coordinate.Zero).Agent.Get(Coordinate.Zero)?.Id);
        Assert.Throws<InvalidOperationException>(() => world.MoveAgent(999, address));
    }

    [Fact]
    public void AgentIdCanExceedTheContainingMapsCellCount()
    {
        var coordinates = Enumerable.Range(0, 5).Select(x => new Coordinate(x, 0)).ToArray();
        var world = CreateWorld(new(1, 1), coordinates[1..]);

        long lastId = -1;
        foreach (var coordinate in coordinates)
        {
            lastId = world.CreateAgent(AgentDefinition, new(coordinate, Coordinate.Zero));
        }

        Assert.Equal(4, lastId);
        Assert.Equal(lastId, world.GetMap(coordinates[^1]).Agent.Get(Coordinate.Zero)?.Id);
    }

    private static World CreateWorld(Coordinate mapSize, params Coordinate[] additionalMapCoordinates)
    {
        var world = new World((uint)(additionalMapCoordinates.Length + 1), 1);
        var style = CreateMapStyle(mapSize);

        world.CreateMap(style, Coordinate.Zero);
        foreach (var coordinate in additionalMapCoordinates)
        {
            world.CreateMap(style, coordinate);
        }

        return world;
    }

    private static MapStyle CreateMapStyle(Coordinate mapSize)
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
