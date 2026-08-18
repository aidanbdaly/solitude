using System;
using System.Collections.Generic;
using Solitude.Domain.Game.Agents.Plans;

namespace Solitude.Domain.Game.Agents.Planning;

public sealed class Planner
{
    private const int MaxExpandedStates = 32768;
    private readonly PlanningModel _model;
    private readonly PlanActionGenerator _actions;

    public Planner(PlanningModel model, PlanActionGenerator actions)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _actions = actions ?? throw new ArgumentNullException(nameof(actions));
    }

    public Plan? FindCheapestPlan(PlanningState initialState, Goal goal)
    {
        ArgumentNullException.ThrowIfNull(initialState);
        ArgumentNullException.ThrowIfNull(goal);
        if (_model.AreSatisfied(initialState, goal.Conditions)) return null;

        var frontier = new PriorityQueue<SearchNode, float>();
        var initial = new SearchNode(initialState, null, null, 0f);
        var bestCosts = new Dictionary<PlanningStateKey, float>
        {
            [initialState.CreateKey()] = 0f
        };
        frontier.Enqueue(initial, 0f);

        var expanded = 0;
        while (frontier.TryDequeue(out var current, out _))
        {
            if (++expanded > MaxExpandedStates)
                throw new InvalidOperationException(
                    $"Planning exceeded {MaxExpandedStates} expanded states for goal '{goal.Name}'.");

            var currentKey = current.State.CreateKey();
            if (bestCosts.TryGetValue(currentKey, out var best)
                && current.Cost > best)
                continue;
            if (_model.AreSatisfied(current.State, goal.Conditions))
                return Reconstruct(current);

            foreach (var action in _actions.Generate(current.State, goal))
            {
                if (!_model.AreSatisfied(current.State, action.Preconditions)) continue;

                var nextState = _model.Apply(current.State, action);
                var nextKey = nextState.CreateKey();
                if (nextKey.Equals(currentKey))
                    throw new InvalidOperationException(
                        $"Planning action {action.GetType().Name} has no observable effect.");
                var nextCost = current.Cost + action.Cost;
                if (bestCosts.TryGetValue(nextKey, out var known) && nextCost >= known)
                    continue;

                bestCosts[nextKey] = nextCost;
                var next = new SearchNode(nextState, current, action, nextCost);
                frontier.Enqueue(next, nextCost);
            }
        }

        return null;
    }

    private static Plan Reconstruct(SearchNode completed)
    {
        var actions = new List<PlanAction>();
        for (var node = completed; node.Action is not null; node = node.Previous!)
            actions.Add(node.Action);
        actions.Reverse();
        return new Plan(actions);
    }

    private sealed record SearchNode(
        PlanningState State,
        SearchNode? Previous,
        PlanAction? Action,
        float Cost);
}
