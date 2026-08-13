using System;
using System.Collections.Generic;
using System.Linq;

namespace Solitude.Domain.Game.Agents.Activity;

public sealed class AgentActivity
{
    private readonly ActivityStep[] _steps;

    public IReadOnlyList<ActivityStep> Steps => _steps;
    public int CurrentStep { get; internal set; }
    public float InstructionTimeRemaining { get; internal set; }
    public ActivityStep Current => CurrentStep >= 0 && CurrentStep < _steps.Length
        ? _steps[CurrentStep]
        : throw new InvalidOperationException($"Activity current step {CurrentStep} is invalid.");

    public AgentActivity(IEnumerable<ActivityStep> steps)
    {
        ArgumentNullException.ThrowIfNull(steps);
        _steps = steps.ToArray();
        if (_steps.Length == 0) throw new ArgumentException("An activity requires at least one step.", nameof(steps));
        ValidateTransitions();
    }

    internal void MoveToStep(int step)
    {
        if (step < 0 || step >= _steps.Length)
            throw new ArgumentOutOfRangeException(nameof(step));
        CurrentStep = step;
        InstructionTimeRemaining = 0f;
    }

    private void ValidateTransitions()
    {
        for (var index = 0; index < _steps.Length; index++)
        {
            ValidateTransition(index, _steps[index].OnSuccess);
            ValidateTransition(index, _steps[index].OnFailure);
        }
    }

    private void ValidateTransition(int source, StepTransition transition)
    {
        if (transition is GoTo { Step: < 0 } invalid)
            throw new ArgumentOutOfRangeException(nameof(transition), $"Step {source} transitions to invalid step {invalid.Step}.");

        var target = transition switch
        {
            Next => source + 1,
            GoTo goTo => goTo.Step,
            Complete or Fail => -1,
            _ => throw new ArgumentOutOfRangeException(nameof(transition), transition, "Unknown activity transition.")
        };
        if (target >= _steps.Length)
            throw new ArgumentOutOfRangeException(nameof(transition), $"Step {source} transitions outside the activity to step {target}.");
    }
}
