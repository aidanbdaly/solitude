using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Solitude.Domain.Game.Orders;

namespace Solitude.Domain.Game.Agents.Planning;

public sealed record Goal
{
    public string Name { get; }
    public ImmutableArray<PlanningCondition> Conditions { get; }

    public Goal(string name, IEnumerable<PlanningCondition> conditions)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A goal requires a name.", nameof(name));
        ArgumentNullException.ThrowIfNull(conditions);

        Name = name;
        Conditions = conditions.ToImmutableArray();
        if (Conditions.IsEmpty)
            throw new ArgumentException("A goal requires at least one condition.", nameof(conditions));
        if (Conditions.Contains(null!))
            throw new ArgumentException("A goal cannot contain a null condition.", nameof(conditions));
    }
}

public sealed record GoalCandidate
{
    public Goal Goal { get; }
    public float Priority { get; }
    public OrderId? SourceOrder { get; }

    public GoalCandidate(Goal goal, float priority, OrderId? sourceOrder = null)
    {
        Goal = goal ?? throw new ArgumentNullException(nameof(goal));
        if (!float.IsFinite(priority) || priority <= 0f)
            throw new ArgumentOutOfRangeException(nameof(priority));
        Priority = priority;
        SourceOrder = sourceOrder;
    }
}
