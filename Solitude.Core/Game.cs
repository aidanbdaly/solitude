public sealed partial class Game(uint worldWidth, uint worldHeight, Coordinate activeMapCoordinate)
{
    public event EventHandler<MapChangedEvent>? MapChanged;

    private Coordinate? _worldCoordinate = activeMapCoordinate;

    public uint Width { get; } = worldWidth;
    public uint Height { get; } = worldHeight;

    private long _nextAgentId;
    private long _nextItemId;

    private readonly Dictionary<long, Agent> _agent = [];
    private readonly Dictionary<long, WorldAddress> _agentAddress = [];

    private readonly Dictionary<long, Item> _item = [];
    private readonly Dictionary<long, WorldAddress> _itemAddress = [];

    private readonly SparseGrid<Map> _map = new(worldWidth, worldWidth);

    public long CreateItem(ItemType type, WorldAddress address)
    {
        var map = GetMap(address.WorldCoordinate);

        var itemId = _nextItemId;
        var item = new Item
        {
            Id = itemId,
            Type = type
        };

        map.SetItem(item, address.MapCoordinate);
        _item.Add(itemId, item);
        _itemAddress.Add(itemId, address);
        _nextItemId++;

        return itemId;
    }

    public long CreateAgent(AgentDefinition definition, WorldAddress address)
    {
        var map = GetMap(address.WorldCoordinate);

        var agentId = _nextAgentId;
        var agent = new Agent
        {
            Id = agentId,
            Definition = definition,
            Status = AgentStatus.Default,
            Inventory = new()
        };

        map.SetAgent(agent, address.MapCoordinate);
        _agent.Add(agentId, agent);
        _agentAddress.Add(agentId, address);
        _nextAgentId++;

        return agentId;
    }

    public long CreateAgent(AgentDefinition definition, Coordinate worldCoordinate)
    {
        var map = GetMap(worldCoordinate);

        var mapCoordinate = map.Occupation
            .First(cell => cell.value == false)
            .coordinate;

        return CreateAgent(definition, new WorldAddress(worldCoordinate, mapCoordinate));
    }

    public void MoveAgent(long agentId, WorldAddress address)
    {
        if (!_agent.TryGetValue(agentId, out var agent) ||
            !_agentAddress.TryGetValue(agentId, out var previousAddress))
        {
            throw new InvalidOperationException($"Cannot move unknown agent '{agentId}'");
        }

        if (previousAddress == address)
        {
            return;
        }

        var destinationMap = GetMap(address.WorldCoordinate);
        var previousMap = GetMap(previousAddress.WorldCoordinate);

        previousMap.RemoveAgent(agent, previousAddress.MapCoordinate);
        destinationMap.SetAgent(agent, address.MapCoordinate);

        _agentAddress[agentId] = address;
    }

    public void CreateMap(MapDefinition style, Coordinate worldCoordinate)
    {
        _map.Set(worldCoordinate, Map.FromDefinition(style));
    }

    public Map GetMap(Coordinate worldCoordinate)
    {
        return _map.Get(worldCoordinate)
               ?? throw new InvalidOperationException($"The map at {worldCoordinate} does not exist");
    }

    public Map GetActiveMap()
    {
        if (_worldCoordinate is Coordinate worldCoordinate)
        {
            return GetMap(worldCoordinate);
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
        CreateMap(request.MapStyle, request.Coordinate);

        foreach (var agent in request.Agents)
        {
            CreateAgent(agent, request.Coordinate);
        }
    }









    internal Coordinate? ActiveWorldCoordinate => _worldCoordinate;

    internal long NextAgentId => _nextAgentId;

    internal long NextItemId => _nextItemId;

    internal IReadOnlyDictionary<long, Agent> Agents => _agent;

    internal IReadOnlyDictionary<long, Item> Items => _item;

    internal IEnumerable<(Coordinate Coordinate, Map Map)> GetMaps()
    {
        foreach (var (coordinate, map) in _map)
        {
            yield return (coordinate, map);
        }
    }

    internal void AddMap(Coordinate coordinate, Map map) => _map.Set(coordinate, map);

    internal void AddAgent(Agent agent) => _agent.Add(agent.Id, agent);

    internal void AddItem(Item item) => _item.Add(item.Id, item);

    internal void PlaceAgent(long agentId, WorldAddress address)
    {
        GetMap(address.WorldCoordinate).SetAgent(_agent[agentId], address.MapCoordinate);
        _agentAddress.Add(agentId, address);
    }

    internal void PlaceItem(long itemId, WorldAddress address)
    {
        GetMap(address.WorldCoordinate).SetItem(_item[itemId], address.MapCoordinate);
        _itemAddress.Add(itemId, address);
    }

    internal void SetNextIds(long nextAgentId, long nextItemId)
    {
        _nextAgentId = nextAgentId;
        _nextItemId = nextItemId;
    }
}
