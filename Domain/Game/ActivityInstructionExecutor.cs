using Godot;
using System;
using System.Linq;
using Solitude.Domain.Game.Agents;
using Solitude.Domain.Game.Agents.Activity;
using Solitude.Domain.Game.Items;
using Solitude.Domain.Game.Map;
using Solitude.Domain.Game;

namespace Solitude.Domain.Game;

public sealed class ActivityInstructionExecutor
{
    private readonly World _world;
    private readonly Action<SimulationEvent> _publish;

    public ActivityInstructionExecutor(
        World world,
        Action<SimulationEvent> publish)
    {
        _world = world;
        _publish = publish;
    }

    public ActivityInstructionResult Execute(
        Agent agent,
        AgentActivity activity,
        ActivityInstruction instruction,
        ref float remainingTime) => instruction switch
    {
        MoveInstruction move => ExecuteMove(agent, move),
        WaitInstruction wait => ExecuteWait(activity, wait, ref remainingTime),
        HasItemCondition condition => agent.Inventory.GetCount(condition.Type) > 0
            ? ActivityInstructionResult.Succeeded
            : ActivityInstructionResult.Failed,
        ObjectExistsCondition condition => _world.TryGetObject(condition.Target, out _)
            ? ActivityInstructionResult.Succeeded
            : ActivityInstructionResult.Failed,
        ConstructionSiteExistsCondition condition =>
            _world.TryGetConstructionSite(condition.Target, out _)
                ? ActivityInstructionResult.Succeeded
                : ActivityInstructionResult.Failed,
        ConstructionRequirementSatisfiedCondition condition =>
            _world.GetConstructionSite(condition.Target).State.IsRequirementSatisfied(condition.Type)
                ? ActivityInstructionResult.Succeeded
                : ActivityInstructionResult.Failed,
        ReserveItemInstruction reserve => ExecuteReserveItem(agent, reserve),
        CollectReservedItemInstruction => ExecuteCollectReservedItem(agent),
        DamageObjectInstruction damage => ExecuteDamageObject(agent, damage),
        SupplyConstructionInstruction supply => ExecuteSupplyConstruction(agent, supply),
        AdvanceConstructionInstruction advance => ExecuteAdvanceConstruction(agent, advance),
        _ => throw new ArgumentOutOfRangeException(
            nameof(instruction), instruction, "Unknown activity instruction.")
    };

    private ActivityInstructionResult ExecuteMove(
        Agent agent,
        MoveInstruction instruction)
    {
        Vector2I target;
        switch (instruction.Target)
        {
            case CellTarget fixedTarget:
                if (!_world.Grid.Contains(fixedTarget.Cell))
                    throw new InvalidOperationException(
                        $"Move target {fixedTarget.Cell} is outside the world.");
                target = fixedTarget.Cell;
                break;
            case ReservedItemTarget:
                var reservation = _world.GetItemReservationForAgent(agent.Id);
                var item = _world.GetItem(reservation.ItemId);
                target = item.Cell;
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(instruction), instruction.Target, "Unknown move target.");
        }

        var navigation = agent.Navigation;
        if (navigation.Status == AgentNavigationStatus.Idle)
        {
            if (!_world.TryFindPath(agent.Cell, target, instruction.GoalMode, out var path))
            {
                navigation.Reset();
                return ActivityInstructionResult.Failed;
            }
            navigation.Begin(target, instruction.GoalMode, path.Skip(1));
        }
        else if (navigation.Target != target || navigation.GoalMode != instruction.GoalMode)
        {
            throw new InvalidOperationException(
                $"Agent {agent.Id.Value} navigation belongs to another move instruction.");
        }

        switch (navigation.Status)
        {
            case AgentNavigationStatus.Moving:
                return ActivityInstructionResult.Running;
            case AgentNavigationStatus.Succeeded:
                navigation.Reset();
                return ActivityInstructionResult.Succeeded;
            case AgentNavigationStatus.Failed:
                navigation.Reset();
                return ActivityInstructionResult.Failed;
            default:
                throw new InvalidOperationException(
                    $"Agent {agent.Id.Value} has invalid navigation status {navigation.Status}.");
        }
    }

    private static ActivityInstructionResult ExecuteWait(
        AgentActivity activity,
        WaitInstruction instruction,
        ref float remainingTime)
    {
        if (!float.IsFinite(activity.InstructionTimeRemaining)
            || activity.InstructionTimeRemaining < 0f)
            throw new InvalidOperationException("Activity contains invalid wait state.");
        if (activity.InstructionTimeRemaining == 0f)
            activity.InstructionTimeRemaining = instruction.Duration;

        var elapsed = Math.Min(activity.InstructionTimeRemaining, remainingTime);
        activity.InstructionTimeRemaining -= elapsed;
        remainingTime -= elapsed;
        if (activity.InstructionTimeRemaining > 0f) return ActivityInstructionResult.Running;

        activity.InstructionTimeRemaining = 0f;
        return ActivityInstructionResult.Succeeded;
    }

    private ActivityInstructionResult ExecuteReserveItem(
        Agent agent,
        ReserveItemInstruction instruction)
    {
        if (_world.TryGetItemReservationForAgent(agent.Id, out _))
            throw new InvalidOperationException(
                $"Agent {agent.Id.Value} already has an item reservation.");

        foreach (var item in _world.Items
                     .Where(item => item.Type == instruction.Type
                         && _world.GetAvailableItemCount(item.Id) > 0)
                     .OrderBy(item => GridMath.Manhattan(agent.Cell, item.Cell)))
        {
            if (!_world.TryFindPath(agent.Cell, item.Cell, PathGoalMode.ExactCell, out _)) continue;
            var count = Math.Min(
                instruction.MaximumCount,
                _world.GetAvailableItemCount(item.Id));
            if (count <= 0)
                throw new InvalidOperationException(
                    $"Available item {item.Id.Value} produced a non-positive reservation.");
            _world.ReserveItem(item.Id, agent.Id, count);
            return ActivityInstructionResult.Succeeded;
        }

        return ActivityInstructionResult.Failed;
    }

    private ActivityInstructionResult ExecuteCollectReservedItem(Agent agent)
    {
        var reservation = _world.GetItemReservationForAgent(agent.Id);
        var item = _world.GetItem(reservation.ItemId);
        if (agent.Cell != item.Cell)
            throw new InvalidOperationException(
                $"Agent {agent.Id.Value} is not at reserved item {item.Id.Value}.");
        if (agent.Inventory.AvailableCapacity <= 0)
            throw new InvalidOperationException(
                $"Agent {agent.Id.Value} has no capacity for its reserved item.");

        var stack = _world.ConsumeItemReservation(
            reservation.Id,
            agent.Inventory.AvailableCapacity);
        if (stack.Count <= 0)
            throw new InvalidOperationException("Reservation consumption produced an empty stack.");
        if (agent.Inventory.Add(stack.Type, stack.Count) != stack.Count)
            throw new InvalidOperationException(
                $"Agent {agent.Id.Value} inventory rejected reserved items.");
        if (item.IsDepleted) _world.RemoveItem(item.Id);
        return ActivityInstructionResult.Succeeded;
    }

    private ActivityInstructionResult ExecuteDamageObject(
        Agent agent,
        DamageObjectInstruction instruction)
    {
        var target = _world.GetObject(instruction.Target);
        if (!GridMath.IsAdjacent(agent.Cell, target.Cell))
            throw new InvalidOperationException(
                $"Agent {agent.Id.Value} is not adjacent to object {target.Id.Value}.");

        var result = _world.DamageObject(instruction.Target, instruction.Amount);
        _publish(new SimulationEvent(
            SimulationEventType.ObjectDamaged,
            result.Cell,
            result.Type));
        if (result.Destroyed)
            _publish(new SimulationEvent(
                SimulationEventType.ObjectDestroyed,
                result.Cell,
                result.Type));
        return ActivityInstructionResult.Succeeded;
    }

    private ActivityInstructionResult ExecuteSupplyConstruction(
        Agent agent,
        SupplyConstructionInstruction instruction)
    {
        var site = _world.GetConstructionSite(instruction.Target);
        if (!GridMath.IsAdjacent(agent.Cell, site.Cell))
            throw new InvalidOperationException(
                $"Agent {agent.Id.Value} is not adjacent to construction site {site.Id.Value}.");
        if (site.State.SupplyFrom(agent.Inventory, instruction.Type) <= 0)
            throw new InvalidOperationException(
                $"Agent {agent.Id.Value} supplied no {instruction.Type} to site {site.Id.Value}.");
        return ActivityInstructionResult.Succeeded;
    }

    private ActivityInstructionResult ExecuteAdvanceConstruction(
        Agent agent,
        AdvanceConstructionInstruction instruction)
    {
        var site = _world.GetConstructionSite(instruction.Target);
        if (!GridMath.IsAdjacent(agent.Cell, site.Cell))
            throw new InvalidOperationException(
                $"Agent {agent.Id.Value} is not adjacent to construction site {site.Id.Value}.");

        var result = _world.AdvanceConstruction(
            instruction.Target,
            instruction.Amount);
        if (result.Completed)
            _publish(new SimulationEvent(
                SimulationEventType.ConstructionCompleted,
                result.Cell));
        return ActivityInstructionResult.Succeeded;
    }

}
