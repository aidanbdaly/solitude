using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Solitude.Domain.Game.Agents;
using Solitude.Domain.Game.Items;

namespace Solitude.Domain.Game;

public sealed partial class World
{
    private int _nextAgentId = 1;
    private readonly Dictionary<AgentId, Agent> _agents = new();

    public IReadOnlyCollection<Agent> Agents => _agents.Values;

    public AgentId CreateAgent(Vector2I cell, string name, AgentDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (!CanOccupy(cell))
            throw new ArgumentException($"Agent cell {cell} cannot be occupied.", nameof(cell));

        var id = new AgentId(_nextAgentId++);
        _agents.Add(id, new Agent
        {
            Id = id,
            Name = name,
            Cell = cell,
            Position = cell,
            Definition = definition,
            Needs = new AgentNeeds(),
            Inventory = new Inventory(definition.InventoryCapacity)
        });
        return id;
    }

    public bool TryGetAgent(AgentId id, out Agent agent) =>
        _agents.TryGetValue(id, out agent!);

    public Agent GetAgent(AgentId id)
    {
        if (!TryGetAgent(id, out var agent))
            throw new KeyNotFoundException($"Agent {id.Value} does not exist.");
        return agent;
    }

    public IEnumerable<Agent> AgentsAt(Vector2I cell) =>
        _agents.Values.Where(agent => agent.Cell == cell);

    public void RemoveAgent(AgentId id)
    {
        var agent = GetAgent(id);
        if (agent.Plan is not null
            || agent.Navigation.Status != AgentNavigationStatus.Idle)
            throw new InvalidOperationException(
                $"Agent {id.Value} cannot be removed while it owns active state.");
        _agents.Remove(id);
    }
}
