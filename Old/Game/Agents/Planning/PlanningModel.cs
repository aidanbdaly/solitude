using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Solitude.Domain.Game.Agents.Plans;

namespace Solitude.Domain.Game.Agents.Planning;

public sealed class PlanningModel
{
    public bool IsSatisfied(PlanningState state, PlanningCondition condition)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(condition);

        return condition switch
        {
            TirednessAtMostCondition tiredness =>
                state.Agent.Tiredness <= tiredness.Maximum,
            SatiationAtLeastCondition satiation =>
                state.Agent.Satiation >= satiation.Minimum,
            HasItemCondition item =>
                state.Agent.InventoryCount(item.Type) >= item.Count,
            InventorySpaceAtLeastCondition space =>
                state.Agent.InventoryAvailableCapacity >= space.Count,
            ItemCountAtLeastCondition item =>
                state.Items.TryGetValue(item.Target, out var available)
                && available.Count >= item.Count,
            ObjectExistsCondition obj => state.Objects.ContainsKey(obj.Target),
            ObjectAbsentCondition obj => !state.Objects.ContainsKey(obj.Target),
            ConstructionSiteExistsCondition site =>
                state.ConstructionSites.ContainsKey(site.Target),
            ConstructionSiteAbsentCondition site =>
                !state.ConstructionSites.ContainsKey(site.Target),
            ConstructionMaterialRemainingAtLeastCondition material =>
                state.ConstructionSites.TryGetValue(material.Target, out var site)
                && site.MissingCount(material.Type) >= material.Count,
            ConstructionFullySuppliedCondition supplied =>
                state.ConstructionSites.TryGetValue(supplied.Target, out var site)
                && site.IsFullySupplied,
            _ => throw new ArgumentOutOfRangeException(
                nameof(condition), condition, "Unknown planning condition.")
        };
    }

    public bool AreSatisfied(
        PlanningState state,
        IEnumerable<PlanningCondition> conditions)
    {
        ArgumentNullException.ThrowIfNull(conditions);
        foreach (var condition in conditions)
        {
            if (!IsSatisfied(state, condition)) return false;
        }
        return true;
    }

    public PlanningState Apply(PlanningState state, PlanAction action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (!AreSatisfied(state, action.Preconditions))
            throw new InvalidOperationException(
                $"Planning action {action.GetType().Name} has unsatisfied preconditions.");
        return Apply(state, action.Effects);
    }

    public PlanningState Apply(
        PlanningState state,
        IEnumerable<PlanningEffect> effects)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(effects);

        var result = state;
        foreach (var effect in effects)
            result = Apply(result, effect);
        return result;
    }

    private static PlanningState Apply(PlanningState state, PlanningEffect effect) =>
        effect switch
        {
            SetAgentLocationEffect location => state with
            {
                Agent = state.Agent with { Location = location.Cell }
            },
            AddAgentItemEffect item => ChangeInventory(state, item.Type, item.Count),
            RemoveAgentItemEffect item => ChangeInventory(state, item.Type, -item.Count),
            DecreaseItemCountEffect item => DecreaseItemCount(state, item),
            SupplyConstructionMaterialEffect material =>
                SupplyConstructionMaterial(state, material),
            RemoveObjectEffect obj => RemoveObject(state, obj),
            CompleteConstructionEffect construction =>
                CompleteConstruction(state, construction),
            IncreaseTirednessEffect tiredness => ChangeTiredness(state, tiredness.Amount),
            DecreaseTirednessEffect tiredness => ChangeTiredness(state, -tiredness.Amount),
            IncreaseSatiationEffect satiation => ChangeSatiation(state, satiation.Amount),
            DecreaseSatiationEffect satiation => ChangeSatiation(state, -satiation.Amount),
            _ => throw new ArgumentOutOfRangeException(
                nameof(effect), effect, "Unknown planning effect.")
        };

    private static PlanningState ChangeInventory(
        PlanningState state,
        Items.ItemType type,
        int change)
    {
        if (change == 0)
            throw new InvalidOperationException("A planning inventory effect cannot be empty.");

        var current = state.Agent.InventoryCount(type);
        var next = checked(current + change);
        if (next < 0 || next > state.Agent.InventoryCapacity)
            throw new InvalidOperationException("A planning inventory effect exceeds inventory bounds.");

        var inventory = next == 0
            ? state.Agent.Inventory.Remove(type)
            : state.Agent.Inventory.SetItem(type, next);
        if (inventory.Values.Sum() > state.Agent.InventoryCapacity)
            throw new InvalidOperationException("A planning inventory effect exceeds inventory capacity.");

        return state with
        {
            Agent = state.Agent with { Inventory = inventory }
        };
    }

    private static PlanningState DecreaseItemCount(
        PlanningState state,
        DecreaseItemCountEffect effect)
    {
        if (effect.Count <= 0)
            throw new InvalidOperationException("A planning item effect requires a positive count.");
        if (!state.Items.TryGetValue(effect.Target, out var item)
            || item.Count < effect.Count)
            throw new InvalidOperationException("A planning item effect exceeds the item count.");

        var remaining = item.Count - effect.Count;
        var items = remaining == 0
            ? state.Items.Remove(effect.Target)
            : state.Items.SetItem(effect.Target, item with { Count = remaining });
        return state with { Items = items };
    }

    private static PlanningState SupplyConstructionMaterial(
        PlanningState state,
        SupplyConstructionMaterialEffect effect)
    {
        if (effect.Count <= 0)
            throw new InvalidOperationException("A planning supply effect requires a positive count.");
        if (!state.ConstructionSites.TryGetValue(effect.Target, out var site))
            throw new InvalidOperationException("A planning supply effect targets a missing site.");

        var missing = site.MissingCount(effect.Type);
        if (missing < effect.Count)
            throw new InvalidOperationException("A planning supply effect exceeds missing materials.");
        var materials = site.MissingMaterials.SetItem(effect.Type, missing - effect.Count);
        return state with
        {
            ConstructionSites = state.ConstructionSites.SetItem(
                effect.Target,
                site with { MissingMaterials = materials })
        };
    }

    private static PlanningState RemoveObject(
        PlanningState state,
        RemoveObjectEffect effect)
    {
        if (!state.Objects.ContainsKey(effect.Target))
            throw new InvalidOperationException("A planning object effect targets a missing object.");
        return state with { Objects = state.Objects.Remove(effect.Target) };
    }

    private static PlanningState CompleteConstruction(
        PlanningState state,
        CompleteConstructionEffect effect)
    {
        if (!state.ConstructionSites.TryGetValue(effect.Target, out var site))
            throw new InvalidOperationException("A planning construction effect targets a missing site.");
        if (!site.IsFullySupplied)
            throw new InvalidOperationException("A planning construction effect targets an unsupplied site.");
        return state with
        {
            ConstructionSites = state.ConstructionSites.Remove(effect.Target)
        };
    }

    private static PlanningState ChangeTiredness(PlanningState state, int amount)
    {
        if (amount == 0)
            throw new InvalidOperationException("A planning tiredness effect cannot be empty.");
        return state with
        {
            Agent = state.Agent with
            {
                Tiredness = Math.Clamp(state.Agent.Tiredness + amount, 0, 100)
            }
        };
    }

    private static PlanningState ChangeSatiation(PlanningState state, int amount)
    {
        if (amount == 0)
            throw new InvalidOperationException("A planning satiation effect cannot be empty.");
        return state with
        {
            Agent = state.Agent with
            {
                Satiation = Math.Clamp(state.Agent.Satiation + amount, 0, 100)
            }
        };
    }
}
