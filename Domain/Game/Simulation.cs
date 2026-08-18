using System;
using Solitude.Domain.Game.Agents;
using Solitude.Domain.Game.Agents.Planning;
using Solitude.Domain.Game.Commands;

namespace Solitude.Domain.Game;

public sealed class Simulation
{
    private const float PlanningStepSeconds = 0.15f;

    private readonly MovementProcess _movement;
    private readonly NeedsProcess _needs;
    private readonly PlanningProcess _planning;
    private readonly PlanExecutionProcess _planExecution;
    private readonly State _state;
    private float _planningAccumulator;

    public PlayerCommandService Commands { get; }
    public event Action<SimulationEvent>? EventOccurred;

    public Simulation(State state)
    {
        _state = state;
        ValidateState(state);

        var projector = new PlanningStateProjector(state.World);
        var model = new PlanningModel();
        var actions = new PlanActionGenerator();
        var planner = new Planner(model, actions);
        var executor = new PlanActionExecutor(state.World, Publish);
        _planExecution = new PlanExecutionProcess(
            state.World,
            projector,
            model,
            executor);
        _planning = new PlanningProcess(
            state.World,
            projector,
            new GoalEvaluator(),
            planner);
        _movement = new MovementProcess(state.World);
        _needs = new NeedsProcess(state.World);
        Commands = new PlayerCommandService(state.World);
    }

    public void Update(float delta)
    {
        _state.Clock.Advance(delta);
        _needs.Step(delta);
        _movement.Step(delta);
        _planningAccumulator += delta;
        while (_planningAccumulator >= PlanningStepSeconds)
        {
            _planningAccumulator -= PlanningStepSeconds;
            _planning.Step();
            _planExecution.Step(PlanningStepSeconds);
        }
    }

    private static void ValidateState(State state)
    {
        foreach (var assignment in state.World.OrderAssignments)
        {
            state.World.GetOrder(assignment.OrderId);
            if (!state.World.TryGetAgent(assignment.AgentId, out var agent))
                throw new InvalidOperationException(
                    $"Order assignment references missing agent {assignment.AgentId.Value}.");
            if (agent.Plan is null)
                throw new InvalidOperationException(
                    $"Agent {assignment.AgentId.Value} has an order assignment but no plan.");
        }
    }

    private void Publish(SimulationEvent simulationEvent) => EventOccurred?.Invoke(simulationEvent);
}
