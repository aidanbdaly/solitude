using System;
using System.Collections.Immutable;
using System.Linq;

public abstract record AgentPlanAction
{
    public ImmutableArray<AgentPlanCondition> Preconditions { get; }
    public ImmutableArray<AgentPlanEffect> Effects { get; }
    public float Cost { get; }

    protected AgentPlanAction(
        ImmutableArray<AgentPlanCondition> preconditions,
        ImmutableArray<AgentPlanEffect> effects,
        float cost)
    {
        if (preconditions.IsDefault)
            throw new ArgumentException("Action preconditions must be initialized.", nameof(preconditions));
        if (effects.IsDefaultOrEmpty)
            throw new ArgumentException("An action requires at least one effect.", nameof(effects));
        if (preconditions.Any(condition => condition is null))
            throw new ArgumentException("Action preconditions cannot contain null.", nameof(preconditions));
        if (effects.Any(effect => effect is null))
            throw new ArgumentException("Action effects cannot contain null.", nameof(effects));
        if (!float.IsFinite(cost) || cost <= 0f)
            throw new ArgumentOutOfRangeException(nameof(cost));

        Preconditions = preconditions;
        Effects = effects;
        Cost = cost;
    }
}