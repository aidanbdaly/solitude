using System;
using System.Linq;
using Solitude.Domain.Game.Agents.Plans;

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

            var state = _projector.Capture(agent.Id);
            var plan = FindBestPlan(state);
            if (plan is not null)
                _world.StartPlan(agent.Id, plan);
        }
    }

    private Plan? FindBestPlan(PlanningState state)
    {
        foreach (var candidates in _goals.Evaluate(state)
                     .GroupBy(candidate => candidate.Priority)
                     .OrderByDescending(group => group.Key))
        {
            Plan? bestPlan = null;
            var bestCost = float.PositiveInfinity;
            foreach (var candidate in candidates
                         .OrderBy(candidate => candidate.SourceOrder?.Value ?? int.MaxValue))
            {
                var plan = _planner.FindCheapestPlan(state, candidate.Goal);
                if (plan is null) continue;

                var cost = plan.Actions.Sum(action => action.Cost);
                if (cost >= bestCost) continue;
                bestPlan = plan;
                bestCost = cost;
            }

            if (bestPlan is not null) return bestPlan;
        }

        return null;
    }
}
