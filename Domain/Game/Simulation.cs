using System;
using Solitude.Domain.Game.Agents;
using Solitude.Domain.Game.Commands;
using Solitude.Domain.Game.Orders;

namespace Solitude.Domain.Game;

public sealed class Simulation
{
    private const float DecisionStepSeconds = 0.15f;

    private readonly MovementService _movement;
    private readonly DecisionService _decision;
    private readonly ActivityService _activity;
    private readonly State _state;
    private float _decisionAccumulator;

    public PlayerCommandService Commands { get; }
    public event Action<SimulationEvent>? EventOccurred;

    public Simulation(State state)
    {
        _state = state;
        ValidateState(state);

        var executor = new ActivityInstructionExecutor(state.World, Publish);
        _activity = new ActivityService(state.World, executor, HandleActivityCompletion);
        _decision = new DecisionService(state.World, _activity);
        _movement = new MovementService(state.World);
        Commands = new PlayerCommandService(state.World, _activity);
    }

    public void Update(float delta)
    {
        _state.Clock.Advance(delta);
        _movement.Step(delta);
        _decisionAccumulator += delta;
        while (_decisionAccumulator >= DecisionStepSeconds)
        {
            _decisionAccumulator -= DecisionStepSeconds;
            _decision.Step();
            _activity.Step(DecisionStepSeconds);
        }
    }

    private void HandleActivityCompletion(ActivityCompletion completion)
    {
        if (!_state.World.TryGetOrderAssignmentForAgent(completion.AgentId, out _)) return;

        if (completion.Outcome == ActivityOutcome.Succeeded)
            _state.World.CompleteAssignedOrder(completion.AgentId);
        else
            _state.World.ReleaseOrderAssignmentForAgent(completion.AgentId);
    }

    private static void ValidateState(State state)
    {
        foreach (var assignment in state.World.OrderAssignments)
        {
            state.World.GetOrder(assignment.OrderId);
            if (!state.World.TryGetAgent(assignment.AgentId, out var agent))
                throw new InvalidOperationException(
                    $"Order assignment references missing agent {assignment.AgentId.Value}.");
            if (agent.Activity is null)
                throw new InvalidOperationException(
                    $"Agent {assignment.AgentId.Value} has an order assignment but no activity.");
        }
    }

    private void Publish(SimulationEvent simulationEvent) => EventOccurred?.Invoke(simulationEvent);
}
