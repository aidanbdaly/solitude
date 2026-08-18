using System;
using System.Collections.Immutable;
using System.Linq;
using Solitude.Domain.Game.Orders;

namespace Solitude.Domain.Game.Agents.Planning;

public sealed class PlanningStateProjector
{
    private const int NeedScale = 100;
    private readonly World _world;

    public PlanningStateProjector(World world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
    }

    public PlanningState Capture(AgentId agentId)
    {
        var agent = _world.GetAgent(agentId);
        var inventory = agent.Inventory.GetStacks()
            .ToImmutableDictionary(stack => stack.Type, stack => stack.Count);
        var planningAgent = new AgentPlanningState(
            agent.Id,
            agent.Cell,
            QuantizeNeed(agent.Needs.Tiredness),
            QuantizeNeed(agent.Needs.Satiation),
            agent.Inventory.Capacity,
            agent.Definition.MovementTilesPerSecond,
            agent.Definition.DamageActionsPerSecond,
            agent.Definition.ConstructionActionsPerSecond,
            inventory);

        var items = _world.Items
            .Select(item => new PlanningItem(
                item.Id,
                item.Type,
                item.Cell,
                _world.GetAvailableItemCount(item.Id)))
            .Where(item => item.AvailableCount > 0)
            .ToImmutableDictionary(item => item.Id);
        var objects = _world.Objects
            .Select(obj => new PlanningObject(
                obj.Id,
                obj.Type,
                obj.Cell,
                obj.HitPoints))
            .ToImmutableDictionary(obj => obj.Id);
        var constructionSites = _world.ConstructionSites
            .Select(site => new PlanningConstructionSite(
                site.Id,
                site.BuildingType,
                site.Cell,
                site.State.ProgressRemaining,
                site.State.Requirements
                    .Select(requirement => requirement.Type)
                    .Distinct()
                    .ToImmutableDictionary(
                        type => type,
                        type => site.State.GetMissingCount(type))))
            .ToImmutableDictionary(site => site.Id);
        var orders = _world.UnassignedOrders
            .OrderBy(order => order.Id.Value)
            .Select(ProjectOrder)
            .ToImmutableArray();
        var navigation = new PlanningNavigationMap(
            _world.Grid.Cells
                .Select(pair => pair.Key)
                .Where(_world.CanOccupy));

        return new PlanningState(
            planningAgent,
            items,
            objects,
            constructionSites,
            orders,
            navigation);
    }

    private static PlanningOrder ProjectOrder(Order order) => order switch
    {
        DamageOrder damage => new PlanningDamageOrder(damage.Id, damage.Target),
        ConstructOrder construct => new PlanningConstructOrder(construct.Id, construct.Target),
        _ => throw new ArgumentOutOfRangeException(
            nameof(order), order, "Unknown order type.")
    };

    private static int QuantizeNeed(float value) =>
        Math.Clamp((int)MathF.Round(value * NeedScale), 0, NeedScale);
}
