using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public class AgentPlan
{
    private readonly ImmutableArray<AgentPlanAction> _actions;
    public IReadOnlyList<AgentPlanAction> Actions => _actions;
    public int CurrentActionIndex { get; private set; }
    public bool CurrentActionStarted { get; private set; }
    public float CurrentActionTimeRemaining { get; internal set; }
    public bool IsComplete => CurrentActionIndex >= _actions.Length;
    internal bool IsFresh =>
        CurrentActionIndex == 0
        && !CurrentActionStarted
        && CurrentActionTimeRemaining == 0f;
    public AgentPlanAction CurrentAction => !IsComplete
        ? _actions[CurrentActionIndex]
        : throw new InvalidOperationException("A completed plan has no current action.");

    public AgentPlan(IEnumerable<AgentPlanAction> actions)
    {
        ArgumentNullException.ThrowIfNull(actions);
        _actions = actions.ToImmutableArray();
        if (_actions.IsEmpty)
            throw new ArgumentException("A plan requires at least one action.", nameof(actions));
        if (_actions.Any(action => action is null))
            throw new ArgumentException("A plan cannot contain a null action.", nameof(actions));
    }

    internal void BeginCurrentAction()
    {
        if (IsComplete)
            throw new InvalidOperationException("A completed plan cannot begin another action.");
        if (CurrentActionStarted)
            throw new InvalidOperationException("The current plan action has already begun.");
        CurrentActionStarted = true;
    }

    internal void Advance()
    {
        if (IsComplete)
            throw new InvalidOperationException("A completed plan cannot advance.");
        if (!CurrentActionStarted)
            throw new InvalidOperationException("An unstarted plan action cannot succeed.");

        CurrentActionIndex++;
        CurrentActionStarted = false;
        CurrentActionTimeRemaining = 0f;
    }
}
