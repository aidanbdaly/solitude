public sealed class GameContext(Game game)
{
    public const uint CycleLength = 600;
    public const uint CycleStart = 200;


    private Game _game = game;
    private GameIndex _index = new();


    public long CreateItem(ItemType type, WorldAddress address)
    {
        var map = GetMap(address.WorldCoordinate);

        var id = _game.NextItemId + 1;
        var item = new Item(id, type, 1);

        _game = _game with
        {
            NextItemId = id,
            Items = [.. _game.Items, item]
        };



        map.SetItem(item, address.MapCoordinate);
        _item.Add(itemId, item);
        _itemAddress.Add(itemId, address);

        return id;
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









}
