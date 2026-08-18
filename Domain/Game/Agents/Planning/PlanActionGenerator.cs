using System;
using System.Collections.Generic;
using System.Linq;
using Solitude.Domain.Game.Agents.Plans;
using Solitude.Domain.Game.Map;

namespace Solitude.Domain.Game.Agents.Planning;

public sealed class PlanActionGenerator
{
    private const float InstantActionCost = 0.001f;

    public IEnumerable<PlanAction> Generate(PlanningState state, Goal goal)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(goal);

        foreach (var condition in goal.Conditions)
        {
            switch (condition)
            {
                case ObjectAbsentCondition obj:
                    foreach (var action in GenerateDamageActions(state, obj))
                        yield return action;
                    break;
                case ConstructionSiteAbsentCondition construction:
                    foreach (var action in GenerateConstructionActions(state, construction))
                        yield return action;
                    break;
            }
        }
    }

    private static IEnumerable<PlanAction> GenerateDamageActions(
        PlanningState state,
        ObjectAbsentCondition goal)
    {
        if (state.Agent.DamageActionsPerSecond <= 0f
            || !state.Objects.TryGetValue(goal.Target, out var target))
            yield break;
        if (!TryTravel(
                state,
                target.Cell,
                PathGoalMode.AdjacentToCell,
                out var destination,
                out var travelCost))
            yield break;

        var cost = travelCost
            + target.HitPoints / state.Agent.DamageActionsPerSecond;
        yield return new DamageObjectAction(
            target.Id,
            target.Cell,
            destination,
            cost);
    }

    private static IEnumerable<PlanAction> GenerateConstructionActions(
        PlanningState state,
        ConstructionSiteAbsentCondition goal)
    {
        if (!state.ConstructionSites.TryGetValue(goal.Target, out var site))
            yield break;

        if (site.IsFullySupplied)
        {
            if (state.Agent.ConstructionActionsPerSecond <= 0f) yield break;
            if (!TryTravel(
                    state,
                    site.Cell,
                    PathGoalMode.AdjacentToCell,
                    out var destination,
                    out var travelCost))
                yield break;
            var cost = travelCost
                + site.ProgressRemaining / state.Agent.ConstructionActionsPerSecond;
            yield return new ConstructAction(
                site.Id,
                site.Cell,
                destination,
                cost);
            yield break;
        }

        foreach (var requirement in site.MissingMaterials
                     .Where(pair => pair.Value > 0)
                     .OrderBy(pair => pair.Key))
        {
            var held = state.Agent.InventoryCount(requirement.Key);
            if (held > 0)
            {
                var count = Math.Min(held, requirement.Value);
                if (TryTravel(
                        state,
                        site.Cell,
                        PathGoalMode.AdjacentToCell,
                        out var destination,
                        out var travelCost))
                    yield return new SupplyConstructionAction(
                        site.Id,
                        requirement.Key,
                        count,
                        site.Cell,
                        destination,
                        travelCost + InstantActionCost);
            }

            var capacity = state.Agent.InventoryAvailableCapacity;
            if (capacity <= 0) continue;
            var neededAfterHeldItems = Math.Max(0, requirement.Value - held);
            if (neededAfterHeldItems == 0) continue;
            foreach (var item in state.Items.Values
                     .Where(item => item.Type == requirement.Key)
                         .OrderBy(item => GridMath.Octile(state.Agent.Location, item.Cell))
                         .ThenBy(item => item.Id.Value))
            {
                var count = Math.Min(
                    Math.Min(item.AvailableCount, capacity),
                    neededAfterHeldItems);
                if (count <= 0) continue;
                if (!TryTravel(
                        state,
                        item.Cell,
                        PathGoalMode.ExactCell,
                        out _,
                        out var travelCost))
                    continue;
                yield return new CollectItemAction(
                    item.Id,
                    item.Type,
                    count,
                    item.Cell,
                    travelCost + InstantActionCost);
            }
        }
    }

    private static bool TryTravel(
        PlanningState state,
        Godot.Vector2I target,
        PathGoalMode goalMode,
        out Godot.Vector2I destination,
        out float cost)
    {
        if (!state.Navigation.TryFindPathCost(
                state.Agent.Location,
                target,
                goalMode,
                out destination,
                out var distance))
        {
            cost = 0f;
            return false;
        }

        cost = distance / state.Agent.MovementTilesPerSecond;
        return true;
    }
}
