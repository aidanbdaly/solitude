using System;
using System.Collections.Generic;
using Godot;

public readonly record struct WorldAddress(
    Vector2I WorldCoordinate,
    Vector2I MapCoordinate
);

public sealed partial class World(uint width, uint height) : Resource
{
    private int _nextAgentId = -1;
    private int _nextItemId = -1;

    private readonly List<Agent> _agent = [];
    private readonly List<Vector2I> _agentWorldCoordinate = [];

    private readonly List<Item> _item = [];
    private readonly List<Vector2I> _itemWorldCoordinate = [];
    
    private readonly SparseGrid<Map> _map = new(width, height);

    public void CreateAgent(AgentDefinition definition, WorldAddress address)
    {
        _agent[_nextAgentId] = new()
        {
            Id = _nextAgentId++,
            Definition = definition,
            Status = AgentStatus.Default,
            Inventory = new()
        };

        SetAgentAddress(_nextAgentId, address);
    }

    public void SetAgentAddress(int agentId, WorldAddress address)
    {
        var map = _map.Get(address.WorldCoordinate) ??
            throw new InvalidOperationException("Call to SetAgentMap() failed: Cannot bind an agent to an ungenerated map");

        map.SetAgent(_agent[agentId], address.MapCoordinate);

        _agentWorldCoordinate[agentId] = address.WorldCoordinate;
    }

    public void CreateMap(MapStyle style, Vector2I worldCoordinate)
    {
        if (_map.Get(worldCoordinate) is not null)
        {
            throw new InvalidOperationException("Call to CreateMap() failed: Map already exists at coordinate");
        }

        Map map = Map.Generate(style);

        _map.Set(worldCoordinate, map);
    }

    public Map GetMap(Vector2I worldCoordinate)
    {
        return _map.Get(worldCoordinate)
            ?? throw new InvalidOperationException("Call to GetMap() failed: Map does not exist for coordinate");
    }
}
