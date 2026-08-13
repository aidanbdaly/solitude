using Godot;
using System;
using System.Linq;
using Solitude.Domain.Game.Agents;
using Solitude.Domain.Game.Agents.Plans;
using Solitude.Domain.Game.Items;
using Solitude.Domain.Game.Map;
using Solitude.Domain.Game;

namespace Solitude.Domain.Game;

public sealed class PlanInstructionExecutor
{
    private readonly World _world;
    private readonly Action<SimulationEvent> _publish;

    public PlanInstructionExecutor(
        World world,
        Action<SimulationEvent> publish)
    {
        _world = world;
        _publish = publish;
    }

    public PlanInstructionResult Execute(
        Agent agent,
        Plan plan,
        PlanInstruction instruction,
        ref float remainingTime) => instruction switch
    {
        MoveInstruction move => ExecuteMove(agent, move),
        WaitInstruction wait => ExecuteWait(plan, wait, ref remainingTime),
        HasItemCondition condition => agent.Inventory.GetCount(condition.Type) > 0
            ? PlanInstructionResult.Succeeded
            : PlanInstructionResult.Failed,
        ObjectExistsCondition condition => _world.TryGetObject(condition.Target, out _)
            ? PlanInstructionResult.Succeeded
            : PlanInstructionResult.Failed,
        ConstructionSiteExistsCondition condition =>
            _world.TryGetConstructionSite(condition.Target, out _)
                ? PlanInstructionResult.Succeeded
                : PlanInstructionResult.Failed,
        ConstructionRequirementSatisfiedCondition condition =>
            _world.GetConstructionSite(condition.Target).State.IsRequirementSatisfied(condition.Type)
                ? PlanInstructionResult.Succeeded
                : PlanInstructionResult.Failed,
        ReserveItemInstruction reserve => ExecuteReserveItem(agent, reserve),
        CollectReservedItemInstruction => ExecuteCollectReservedItem(agent),
        DamageObjectInstruction damage => ExecuteDamageObject(agent, damage),
        SupplyConstructionInstruction supply => ExecuteSupplyConstruction(agent, supply),
        AdvanceConstructionInstruction advance => ExecuteAdvanceConstruction(agent, advance),
        _ => throw new ArgumentOutOfRangeException(
            nameof(instruction), instruction, "Unknown plan instruction.")
    };

    private PlanInstructionResult ExecuteMove(
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
                return PlanInstructionResult.Failed;
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
                return PlanInstructionResult.Running;
            case AgentNavigationStatus.Succeeded:
                navigation.Reset();
                return PlanInstructionResult.Succeeded;
            case AgentNavigationStatus.Failed:
                navigation.Reset();
                return PlanInstructionResult.Failed;
            default:
                throw new InvalidOperationException(
                    $"Agent {agent.Id.Value} has invalid navigation status {navigation.Status}.");
        }
    }

    private static PlanInstructionResult ExecuteWait(
        Plan plan,
        WaitInstruction instruction,
        ref float remainingTime)
    {
        if (!float.IsFinite(plan.InstructionTimeRemaining)
            || plan.InstructionTimeRemaining < 0f)
            throw new InvalidOperationException("Plan contains invalid wait state.");
        if (plan.InstructionTimeRemaining == 0f)
            plan.InstructionTimeRemaining = instruction.Duration;

        var elapsed = Math.Min(plan.InstructionTimeRemaining, remainingTime);
        plan.InstructionTimeRemaining -= elapsed;
        remainingTime -= elapsed;
        if (plan.InstructionTimeRemaining > 0f) return PlanInstructionResult.Running;

        plan.InstructionTimeRemaining = 0f;
        return PlanInstructionResult.Succeeded;
    }

    private PlanInstructionResult ExecuteReserveItem(
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
            return PlanInstructionResult.Succeeded;
        }

        return PlanInstructionResult.Failed;
    }

    private PlanInstructionResult ExecuteCollectReservedItem(Agent agent)
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
        return PlanInstructionResult.Succeeded;
    }

    private PlanInstructionResult ExecuteDamageObject(
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
        return PlanInstructionResult.Succeeded;
    }

    private PlanInstructionResult ExecuteSupplyConstruction(
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
        return PlanInstructionResult.Succeeded;
    }

    private PlanInstructionResult ExecuteAdvanceConstruction(
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
        return PlanInstructionResult.Succeeded;
    }

}
