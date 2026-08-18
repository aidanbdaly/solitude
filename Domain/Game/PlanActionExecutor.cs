using Godot;
using System;
using System.Linq;
using Solitude.Domain.Game.Agents;
using Solitude.Domain.Game.Agents.Plans;
using Solitude.Domain.Game.Map;

namespace Solitude.Domain.Game;

public enum PlanActionResult
{
    Running,
    Succeeded,
    Failed
}

public sealed class PlanActionExecutor
{
    private readonly World _world;
    private readonly Action<SimulationEvent> _publish;

    public PlanActionExecutor(World world, Action<SimulationEvent> publish)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _publish = publish ?? throw new ArgumentNullException(nameof(publish));
    }

    public PlanActionResult Execute(
        Agent agent,
        Plan plan,
        PlanAction action,
        ref float remainingTime) => action switch
        {
            MoveToAction move => Navigate(agent, move.Target, PathGoalMode.ExactCell),
            CollectItemAction collect => ExecuteCollect(agent, collect),
            SupplyConstructionAction supply => ExecuteSupply(agent, supply),
            DamageObjectAction damage => ExecuteDamage(agent, plan, damage, ref remainingTime),
            ConstructAction construct => ExecuteConstruct(agent, plan, construct, ref remainingTime),
            _ => throw new ArgumentOutOfRangeException(
                nameof(action), action, "Unknown plan action.")
        };

    private PlanActionResult ExecuteCollect(Agent agent, CollectItemAction action)
    {
        if (!_world.TryGetItemReservationForAgent(agent.Id, out var reservation))
        {
            if (!_world.TryGetItem(action.Target, out var available)
                || available.Type != action.Type
                || available.Cell != action.Cell
                || _world.GetAvailableItemCount(action.Target) < action.Count
                || agent.Inventory.AvailableCapacity < action.Count)
                return PlanActionResult.Failed;

            _world.ReserveItem(action.Target, agent.Id, action.Count);
            reservation = _world.GetItemReservationForAgent(agent.Id);
        }
        else if (reservation.ItemId != action.Target || reservation.Count != action.Count)
        {
            throw new InvalidOperationException(
                $"Agent {agent.Id.Value} reservation does not belong to its collect action.");
        }

        if (!_world.TryGetItem(action.Target, out var item)
            || item.Type != action.Type
            || item.Cell != action.Cell)
            return PlanActionResult.Failed;

        var navigation = Navigate(agent, action.Cell, PathGoalMode.ExactCell);
        if (navigation != PlanActionResult.Succeeded) return navigation;

        if (agent.Inventory.AvailableCapacity < action.Count)
            return PlanActionResult.Failed;
        var stack = _world.ConsumeItemReservation(reservation.Id, action.Count);
        if (stack.Type != action.Type || stack.Count != action.Count)
            throw new InvalidOperationException(
                $"Agent {agent.Id.Value} reservation did not produce its planned item stack.");
        if (agent.Inventory.Add(stack.Type, stack.Count) != stack.Count)
            throw new InvalidOperationException(
                $"Agent {agent.Id.Value} inventory rejected its planned item stack.");
        if (item.IsDepleted) _world.RemoveItem(item.Id);
        return PlanActionResult.Succeeded;
    }

    private PlanActionResult ExecuteSupply(
        Agent agent,
        SupplyConstructionAction action)
    {
        if (!_world.TryGetConstructionSite(action.Target, out var site)
            || site.Cell != action.TargetCell
            || site.State.GetMissingCount(action.Type) < action.Count
            || agent.Inventory.GetCount(action.Type) < action.Count)
            return PlanActionResult.Failed;

        var navigation = Navigate(agent, action.Destination, PathGoalMode.ExactCell);
        if (navigation != PlanActionResult.Succeeded) return navigation;
        if (!GridMath.IsAdjacent(agent.Cell, action.TargetCell))
            throw new InvalidOperationException(
                $"Agent {agent.Id.Value} reached an invalid construction supply destination.");

        var supplied = _world.SupplyConstructionMaterial(
            action.Target,
            agent.Id,
            action.Type,
            action.Count);
        if (supplied != action.Count)
            throw new InvalidOperationException(
                $"Agent {agent.Id.Value} did not supply its planned material count.");
        return PlanActionResult.Succeeded;
    }

    private PlanActionResult ExecuteDamage(
        Agent agent,
        Plan plan,
        DamageObjectAction action,
        ref float remainingTime)
    {
        if (!_world.TryGetObject(action.Target, out var target))
        {
            agent.Navigation.Reset();
            return PlanActionResult.Succeeded;
        }
        if (target.Cell != action.TargetCell)
            return PlanActionResult.Failed;

        var navigation = Navigate(agent, action.Destination, PathGoalMode.ExactCell);
        if (navigation != PlanActionResult.Succeeded) return navigation;
        if (!GridMath.IsAdjacent(agent.Cell, action.TargetCell))
            throw new InvalidOperationException(
                $"Agent {agent.Id.Value} reached an invalid damage destination.");
        if (!ConsumeDelay(plan, ref remainingTime)) return PlanActionResult.Running;

        var rate = agent.Definition.DamageActionsPerSecond;
        if (rate <= 0f)
            throw new InvalidOperationException(
                $"Agent {agent.Id.Value} cannot execute a damage action.");
        var result = _world.DamageObject(action.Target, 1);
        _publish(new SimulationEvent(
            SimulationEventType.ObjectDamaged,
            result.Cell,
            result.Type));
        if (result.Destroyed)
        {
            _publish(new SimulationEvent(
                SimulationEventType.ObjectDestroyed,
                result.Cell,
                result.Type));
            return PlanActionResult.Succeeded;
        }

        plan.CurrentActionTimeRemaining = 1f / rate;
        return PlanActionResult.Running;
    }

    private PlanActionResult ExecuteConstruct(
        Agent agent,
        Plan plan,
        ConstructAction action,
        ref float remainingTime)
    {
        if (!_world.TryGetConstructionSite(action.Target, out var site))
        {
            agent.Navigation.Reset();
            return PlanActionResult.Succeeded;
        }
        if (site.Cell != action.TargetCell || !site.State.IsFullySupplied)
            return PlanActionResult.Failed;

        var navigation = Navigate(agent, action.Destination, PathGoalMode.ExactCell);
        if (navigation != PlanActionResult.Succeeded) return navigation;
        if (!GridMath.IsAdjacent(agent.Cell, action.TargetCell))
            throw new InvalidOperationException(
                $"Agent {agent.Id.Value} reached an invalid construction destination.");
        if (!ConsumeDelay(plan, ref remainingTime)) return PlanActionResult.Running;

        var rate = agent.Definition.ConstructionActionsPerSecond;
        if (rate <= 0f)
            throw new InvalidOperationException(
                $"Agent {agent.Id.Value} cannot execute a construction action.");
        var result = _world.AdvanceConstruction(action.Target, 1);
        if (result.Completed)
        {
            _publish(new SimulationEvent(
                SimulationEventType.ConstructionCompleted,
                result.Cell));
            return PlanActionResult.Succeeded;
        }

        plan.CurrentActionTimeRemaining = 1f / rate;
        return PlanActionResult.Running;
    }

    private PlanActionResult Navigate(
        Agent agent,
        Vector2I target,
        PathGoalMode goalMode)
    {
        var navigation = agent.Navigation;
        if (navigation.Status == AgentNavigationStatus.Idle)
        {
            if (!_world.TryFindPath(agent.Cell, target, goalMode, out var path))
                return PlanActionResult.Failed;
            navigation.Begin(target, goalMode, path.Skip(1));
        }
        else if (navigation.Target != target || navigation.GoalMode != goalMode)
        {
            throw new InvalidOperationException(
                $"Agent {agent.Id.Value} navigation belongs to another plan action.");
        }

        switch (navigation.Status)
        {
            case AgentNavigationStatus.Moving:
                return PlanActionResult.Running;
            case AgentNavigationStatus.Succeeded:
                navigation.Reset();
                return PlanActionResult.Succeeded;
            case AgentNavigationStatus.Failed:
                navigation.Reset();
                return PlanActionResult.Failed;
            default:
                throw new InvalidOperationException(
                    $"Agent {agent.Id.Value} has invalid navigation status {navigation.Status}.");
        }
    }

    private static bool ConsumeDelay(Plan plan, ref float remainingTime)
    {
        if (!float.IsFinite(plan.CurrentActionTimeRemaining)
            || plan.CurrentActionTimeRemaining < 0f)
            throw new InvalidOperationException("Plan contains invalid action timing state.");
        if (plan.CurrentActionTimeRemaining == 0f) return true;

        var elapsed = Math.Min(plan.CurrentActionTimeRemaining, remainingTime);
        plan.CurrentActionTimeRemaining -= elapsed;
        remainingTime -= elapsed;
        return plan.CurrentActionTimeRemaining == 0f;
    }
}
