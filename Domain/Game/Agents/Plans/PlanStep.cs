namespace Solitude.Domain.Game.Agents.Plans;

public enum PlanInstructionResult
{
    Running,
    Succeeded,
    Failed
}

public sealed record PlanStep
{
    public PlanInstruction Instruction { get; }
    public StepTransition OnSuccess { get; }
    public StepTransition OnFailure { get; }

    public PlanStep(
        PlanInstruction instruction,
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
