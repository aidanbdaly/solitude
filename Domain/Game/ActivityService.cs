using System;
using Solitude.Domain.Game.Agents;
using Solitude.Domain.Game.Agents.Activity;
using Solitude.Domain.Game;

namespace Solitude.Domain.Game;

public enum ActivityOutcome
{
    Succeeded,
    Failed,
    Cancelled
}

public readonly record struct ActivityCompletion(
    AgentId AgentId,
    ActivityOutcome Outcome);

public sealed class ActivityService
{
    private const int MaxTransitionsPerStep = 64;

    private readonly World _world;
    private readonly ActivityInstructionExecutor _executor;
    private readonly Action<ActivityCompletion> _onCompleted;

    public ActivityService(
        World world,
        ActivityInstructionExecutor executor,
        Action<ActivityCompletion> onCompleted)
    {
        _world = world;
        _executor = executor;
        _onCompleted = onCompleted;
    }

    public void Start(Agent agent, AgentActivity activity)
    {
        if (agent.Activity is not null)
            throw new InvalidOperationException($"Agent {agent.Id.Value} already has an activity.");
        agent.BeginActivity(activity);
    }

    public void Replace(Agent agent, AgentActivity activity)
    {
        if (agent.Activity is not null) Finish(agent, ActivityOutcome.Cancelled);
        Start(agent, activity);
    }

    public void Cancel(Agent agent)
    {
        if (agent.Activity is not null) Finish(agent, ActivityOutcome.Cancelled);
    }

    public void Step(float delta)
    {
        if (!float.IsFinite(delta) || delta < 0f)
            throw new ArgumentOutOfRangeException(nameof(delta));
        foreach (var agent in _world.Agents)
        {
            if (agent.Activity is not null) Step(agent, delta);
        }
    }

    private void Step(Agent agent, float delta)
    {
        var remainingTime = delta;
        for (var transitions = 0; transitions < MaxTransitionsPerStep; transitions++)
        {
            var activity = agent.Activity;
            if (activity is null) return;
            var step = activity.Current;
            var result = _executor.Execute(
                agent, activity, step.Instruction, ref remainingTime);
            StepTransition transition;
            switch (result)
            {
                case ActivityInstructionResult.Running:
                    return;
                case ActivityInstructionResult.Succeeded:
                    transition = step.OnSuccess;
                    break;
                case ActivityInstructionResult.Failed:
                    transition = step.OnFailure;
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Instruction returned unknown result {result}.");
            }
            switch (transition)
            {
                case Next:
                    activity.MoveToStep(activity.CurrentStep + 1);
                    break;
                case GoTo goTo:
                    activity.MoveToStep(goTo.Step);
                    break;
                case Complete:
                    Finish(agent, ActivityOutcome.Succeeded);
                    return;
                case Fail:
                    Finish(agent, ActivityOutcome.Failed);
                    return;
                default:
                    throw new InvalidOperationException(
                        $"Activity contains unknown transition {transition.GetType().Name}.");
            }
        }

        throw new InvalidOperationException(
            $"Activity for agent {agent.Id.Value} exceeded {MaxTransitionsPerStep} instantaneous transitions.");
    }

    private void Finish(Agent agent, ActivityOutcome outcome)
    {
        var activity = agent.Activity;
        if (activity is null) return;

        agent.Navigation.Reset();
        activity.InstructionTimeRemaining = 0f;
        _world.TryReleaseItemReservationForAgent(agent.Id);
        _onCompleted(new ActivityCompletion(agent.Id, outcome));
        agent.ClearActivity();
    }
}
