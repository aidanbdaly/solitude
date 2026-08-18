using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Solitude.Domain.Game.Agents.Planning;

public sealed class GoalEvaluator
{
    private const float OrderPriority = 1f;

    public IReadOnlyList<GoalCandidate> Evaluate(PlanningState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var candidates = ImmutableArray.CreateBuilder<GoalCandidate>();

        foreach (var order in state.Orders)
        {
            candidates.Add(order switch
            {
                PlanningDamageOrder damage => CreateDamageGoal(state, damage),
                PlanningConstructOrder construct => CreateConstructGoal(state, construct),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(order), order, "Unknown planning order type.")
            });
        }

        return candidates.ToImmutable();
    }

    private static GoalCandidate CreateDamageGoal(
        PlanningState state,
        PlanningDamageOrder order)
    {
        if (!state.Objects.ContainsKey(order.Target))
            throw new InvalidOperationException(
                $"Damage order {order.Id.Value} targets missing object {order.Target.Value}.");
        return new GoalCandidate(
            new Goal(
                $"Destroy object {order.Target.Value}",
                new PlanningCondition[] { new ObjectAbsentCondition(order.Target) }),
            OrderPriority,
            order.Id);
    }

    private static GoalCandidate CreateConstructGoal(
        PlanningState state,
        PlanningConstructOrder order)
    {
        if (!state.ConstructionSites.ContainsKey(order.Target))
            throw new InvalidOperationException(
                $"Construct order {order.Id.Value} targets missing site {order.Target.Value}.");
        return new GoalCandidate(
            new Goal(
                $"Complete construction {order.Target.Value}",
                new PlanningCondition[] { new ConstructionSiteAbsentCondition(order.Target) }),
            OrderPriority,
            order.Id);
    }
}
