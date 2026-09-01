public sealed partial class Game
{
    public event EventHandler<MapChangedEvent>? MapChanged;

    private readonly Player _player;
    private readonly World _world;
    private Coordinate? _worldCoordinate;

    public Game(uint worldWidth, uint worldHeight)
        : this(new(), new(worldWidth, worldHeight), null)
    {
    }

    internal Game(Player player, World world, Coordinate? worldCoordinate)
    {
        _player = player;
        _world = world;
        _worldCoordinate = worldCoordinate;
    }

    internal Player Player => _player;

    internal World World => _world;

    internal Coordinate? ActiveWorldCoordinate => _worldCoordinate;

    public Map GetActiveMap()
    {
        if (_worldCoordinate is Coordinate worldCoordinate)
        {
            return _world.GetMap(worldCoordinate);
        }
        else
        {
            throw new InvalidOperationException("No active map Id");
        }
    }

    public void SetActiveMap(Coordinate worldCoordinate)
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
            _world.CreateAgent(agent, new(request.Coordinate, Coordinate.One));
        }
    }

}
