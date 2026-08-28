using System;
using Godot;

public sealed class MapChangedEvent : EventArgs
{
    public required Map NewMap { get; init; }
}

public sealed partial class Game
{
    public event EventHandler<MapChangedEvent>? MapChanged;

    private readonly Player _player;
    private readonly World _world;
    private Vector2I? _worldCoordinate;

    public Game(uint worldWidth, uint worldHeight)
        : this(new(), new(worldWidth, worldHeight), null)
    {
    }

    internal Game(Player player, World world, Vector2I? worldCoordinate)
    {
        _player = player;
        _world = world;
        _worldCoordinate = worldCoordinate;
    }

    internal Player Player => _player;

    internal World World => _world;

    internal Vector2I? ActiveWorldCoordinate => _worldCoordinate;

    public Map GetActiveMap()
    {
        if (_worldCoordinate is Vector2I worldCoordinate)
        {
            return _world.GetMap(worldCoordinate);
        }
        else
        {
            throw new InvalidOperationException("No active map Id");
        }
    }

    public void SetActiveMap(Vector2I worldCoordinate)
    {
        _worldCoordinate = worldCoordinate;

        MapChanged?.Invoke(this, new()
        {
            NewMap = GetActiveMap()
        });
    }

    public void CreatePopulatedMap(CreatePopulatedMapRequest request)
    {
        _world.CreateMap(request.MapStyle, request.Coordinate);

        foreach (var agent in request.Agents)
        {
            _world.CreateAgent(agent, new(request.Coordinate, Vector2I.One));
        }
    }

}
