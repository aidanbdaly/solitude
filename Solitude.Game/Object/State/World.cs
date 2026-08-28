using System;
using System.Collections.Generic;
using Godot;

public readonly record struct WorldAddress(
    Vector2I WorldCoordinate,
    Vector2I MapCoordinate
);

public sealed class World(uint width, uint height)
{
    public uint Width { get; } = width;
    public uint Height { get; } = height;

    private long _nextAgentId;
    private long _nextItemId;

    private readonly Dictionary<long, Agent> _agent = [];
    private readonly Dictionary<long, WorldAddress> _agentAddress = [];

    private readonly Dictionary<long, Item> _item = [];
    private readonly Dictionary<long, WorldAddress> _itemAddress = [];

    private readonly SparseGrid<Map> _map = new(width, height);

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

    public void CreateMap(MapStyle style, Vector2I worldCoordinate)
    {
        _map.Set(worldCoordinate, Map.Generate(style));
    }

    public Map GetMap(Vector2I worldCoordinate)
    {
        return _map.Get(worldCoordinate)
            ?? throw new InvalidOperationException("Call to GetMap() failed: Map does not exist for coordinate");
    }

    internal long NextAgentId => _nextAgentId;

    internal long NextItemId => _nextItemId;

    internal IReadOnlyDictionary<long, Agent> Agents => _agent;

    internal IReadOnlyDictionary<long, Item> Items => _item;

    internal IEnumerable<(Vector2I Coordinate, Map Map)> GetMaps()
    {
        foreach (var (coordinate, map) in _map)
        {
            yield return (coordinate, map);
        }
    }

    internal void AddMap(Vector2I coordinate, Map map) => _map.Set(coordinate, map);

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
