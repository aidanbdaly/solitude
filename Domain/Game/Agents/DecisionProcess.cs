using System;
using Solitude.Domain.Game;
using Solitude.Domain.Game.Agents.Plans;
using Solitude.Domain.Game.Orders;

namespace Solitude.Domain.Game.Agents;

public sealed class DecisionProcess
{
    private readonly World _world;

    public DecisionProcess(World world)
    {
        _world = world;
    }

    public void Step()
    {
        foreach (var agent in _world.Agents)
        {
            if (agent.Plan is not null) continue;
            if (_world.TryGetOrderAssignmentForAgent(agent.Id, out _))
                throw new InvalidOperationException(
                    $"Idle agent {agent.Id.Value} retains an order assignment.");
            Deliberate(agent);
        }
    }

    private void Deliberate(Agent agent)
    {
        foreach (var order in _world.UnassignedOrders)
        {
            var plan = CreatePlanIfCapable(order, agent);
            if (plan is null) continue;
            _world.AssignOrder(order.Id, agent.Id);
            _world.StartPlan(agent.Id, plan);
            return;
        }
    }

    private Plan? CreatePlanIfCapable(Order order, Agent agent)
    {
        switch (order)
        {
            case DamageOrder damage:
            {
                var target = _world.GetObject(damage.Target);
                var rate = agent.Definition.DamageActionsPerSecond;
                return rate > 0f
                    ? PlanComposer.ComposeDamage(target, 1f / rate)
                    : null;
            }
            case ConstructOrder construct:
            {
                var site = _world.GetConstructionSite(construct.Target);
                var definition = agent.Definition;
                if (definition.ConstructionActionsPerSecond <= 0f
                    || (!site.State.IsFullySupplied && definition.InventoryCapacity <= 0))
                    return null;
                return PlanComposer.ComposeConstruct(
                    site,
                    definition.InventoryCapacity,
                    1f / definition.ConstructionActionsPerSecond);
            }
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(order),
                    order,
                    "Unknown order type.");
        }
    }
}
