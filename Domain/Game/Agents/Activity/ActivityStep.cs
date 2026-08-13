namespace Solitude.Domain.Game.Agents.Activity;

public enum ActivityInstructionResult
{
    Running,
    Succeeded,
    Failed
}

public sealed record ActivityStep
{
    public ActivityInstruction Instruction { get; }
    public StepTransition OnSuccess { get; }
    public StepTransition OnFailure { get; }

    public ActivityStep(
        ActivityInstruction instruction,
        StepTransition onSuccess,
        StepTransition onFailure)
    {
        Instruction = instruction ?? throw new System.ArgumentNullException(nameof(instruction));
        OnSuccess = onSuccess ?? throw new System.ArgumentNullException(nameof(onSuccess));
        OnFailure = onFailure ?? throw new System.ArgumentNullException(nameof(onFailure));
    }
}

public abstract record StepTransition;
public sealed record Next : StepTransition;
public sealed record GoTo(int Step) : StepTransition;
public sealed record Complete : StepTransition;
public sealed record Fail : StepTransition;
