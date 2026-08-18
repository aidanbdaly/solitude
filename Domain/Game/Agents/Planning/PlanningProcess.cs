using System;
using System.Linq;

namespace Solitude.Domain.Game.Agents.Planning;

public sealed class PlanningProcess
{
    private readonly World _world;
    private readonly PlanningStateProjector _projector;
    private readonly GoalEvaluator _goals;
    private readonly Planner _planner;

    public PlanningProcess(
        World world,
        PlanningStateProjector projector,
        GoalEvaluator goals,
        Planner planner)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _projector = projector ?? throw new ArgumentNullException(nameof(projector));
        _goals = goals ?? throw new ArgumentNullException(nameof(goals));
        _planner = planner ?? throw new ArgumentNullException(nameof(planner));
    }

    public void Step()
    {
        foreach (var agent in _world.Agents)
        {
            if (agent.Plan is not null) continue;
            if (_world.TryGetOrderAssignmentForAgent(agent.Id, out _))
                throw new InvalidOperationException(
                    $"Idle agent {agent.Id.Value} retains an order assignment.");

            var state = _projector.Capture(agent.Id);
            foreach (var candidate in _goals.Evaluate(state)
                         .OrderByDescending(candidate => candidate.Priority)
                         .ThenBy(candidate => candidate.SourceOrder?.Value ?? int.MaxValue))
            {
                var plan = _planner.FindCheapestPlan(state, candidate.Goal);
                if (plan is null) continue;

                if (candidate.SourceOrder is { } orderId)
                    _world.AssignOrder(orderId, agent.Id);
                _world.StartPlan(agent.Id, plan);
                break;
            }
        }
    }
}
