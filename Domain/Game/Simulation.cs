using System;
using Solitude.Domain.Game.Agents;
using Solitude.Domain.Game.Commands;

namespace Solitude.Domain.Game;

public sealed class Simulation
{
    private const float DecisionStepSeconds = 0.15f;

    private readonly MovementProcess _movement;
    private readonly DecisionProcess _decision;
    private readonly PlanExecutionProcess _planExecution;
    private readonly State _state;
    private float _decisionAccumulator;

    public PlayerCommandService Commands { get; }
    public event Action<SimulationEvent>? EventOccurred;

    public Simulation(State state)
    {
        _state = state;
        ValidateState(state);

        var executor = new PlanInstructionExecutor(state.World, Publish);
        _planExecution = new PlanExecutionProcess(state.World, executor);
        _decision = new DecisionProcess(state.World);
        _movement = new MovementProcess(state.World);
        Commands = new PlayerCommandService(state.World);
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
            _planExecution.Step(DecisionStepSeconds);
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
