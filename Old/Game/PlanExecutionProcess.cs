using System;
using Solitude.Domain.Game.Agents;
using Solitude.Domain.Game.Agents.Planning;
using Solitude.Domain.Game.Agents.Plans;

namespace Solitude.Domain.Game;

public sealed class PlanExecutionProcess
{
    private const int MaxActionsPerStep = 64;

    private readonly World _world;
    private readonly PlanningStateProjector _projector;
    private readonly PlanningModel _model;
    private readonly PlanActionExecutor _executor;

    public PlanExecutionProcess(
        World world,
        PlanningStateProjector projector,
        PlanningModel model,
        PlanActionExecutor executor)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _projector = projector ?? throw new ArgumentNullException(nameof(projector));
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _executor = executor ?? throw new ArgumentNullException(nameof(executor));
    }

    public void Step(float delta)
    {
        if (!float.IsFinite(delta) || delta < 0f)
            throw new ArgumentOutOfRangeException(nameof(delta));
        if (delta == 0f) return;

        foreach (var agent in _world.Agents)
        {
            if (agent.Plan is not null) Step(agent, delta);
        }
    }

    private void Step(Agent agent, float delta)
    {
        var remainingTime = delta;
        for (var actions = 0; actions < MaxActionsPerStep; actions++)
        {
            var plan = agent.Plan;
            if (plan is null) return;

            var action = plan.CurrentAction;
            if (!plan.CurrentActionStarted)
            {
                var state = _projector.Capture(agent.Id);
                if (!_model.AreSatisfied(state, action.Preconditions))
                {
                    _world.FinishPlan(agent.Id);
                    return;
                }
                plan.BeginCurrentAction();
            }

            switch (_executor.Execute(agent, plan, action, ref remainingTime))
            {
                case PlanActionResult.Running:
                    return;
                case PlanActionResult.Failed:
                    _world.FinishPlan(agent.Id);
                    return;
                case PlanActionResult.Succeeded:
                    if (agent.Navigation.Status != AgentNavigationStatus.Idle)
                        throw new InvalidOperationException(
                            $"Plan action {action.GetType().Name} succeeded with active navigation.");
                    plan.Advance();
                    if (plan.IsComplete)
                    {
                        _world.FinishPlan(agent.Id);
                        return;
                    }
                    break;
                default:
                    throw new InvalidOperationException(
                        "Plan action returned an unknown result.");
            }
        }

        throw new InvalidOperationException(
            $"Plan for agent {agent.Id.Value} exceeded "
            + $"{MaxActionsPerStep} instantaneous actions.");
    }
}
