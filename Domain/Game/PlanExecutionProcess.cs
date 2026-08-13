using System;
using Solitude.Domain.Game.Agents;
using Solitude.Domain.Game.Agents.Plans;

namespace Solitude.Domain.Game;

public sealed class PlanExecutionProcess
{
    private const int MaxTransitionsPerStep = 64;

    private readonly World _world;
    private readonly PlanInstructionExecutor _executor;

    public PlanExecutionProcess(World world, PlanInstructionExecutor executor)
    {
        _world = world;
        _executor = executor;
    }

    public void Step(float delta)
    {
        if (!float.IsFinite(delta) || delta < 0f)
            throw new ArgumentOutOfRangeException(nameof(delta));

        foreach (var agent in _world.Agents)
        {
            if (agent.Plan is not null) Step(agent, delta);
        }
    }

    private void Step(Agent agent, float delta)
    {
        var remainingTime = delta;
        for (var transitions = 0; transitions < MaxTransitionsPerStep; transitions++)
        {
            var plan = agent.Plan;
            if (plan is null) return;

            var step = plan.Current;
            var result = _executor.Execute(
                agent,
                plan,
                step.Instruction,
                ref remainingTime);
            var transition = result switch
            {
                PlanInstructionResult.Running => null,
                PlanInstructionResult.Succeeded => step.OnSuccess,
                PlanInstructionResult.Failed => step.OnFailure,
                _ => throw new InvalidOperationException(
                    $"Instruction returned unknown result {result}.")
            };
            if (transition is null) return;

            switch (transition)
            {
                case Next:
                    plan.MoveToStep(plan.CurrentStep + 1);
                    break;
                case GoTo goTo:
                    plan.MoveToStep(goTo.Step);
                    break;
                case Complete:
                    _world.FinishPlan(agent.Id, PlanOutcome.Succeeded);
                    return;
                case Fail:
                    _world.FinishPlan(agent.Id, PlanOutcome.Failed);
                    return;
                default:
                    throw new InvalidOperationException(
                        $"Plan contains unknown transition {transition.GetType().Name}.");
            }
        }

        throw new InvalidOperationException(
            $"Plan for agent {agent.Id.Value} exceeded "
            + $"{MaxTransitionsPerStep} instantaneous transitions.");
    }
}
