using System;
using Solitude.Domain.Game.Agents;
using Solitude.Domain.Game.Agents.Plans;

namespace Solitude.Domain.Game;

public sealed partial class World
{
    public void StartPlan(AgentId agentId, Plan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var agent = GetAgent(agentId);
        if (agent.Plan is not null)
            throw new InvalidOperationException(
                $"Agent {agent.Id.Value} already has a plan.");
        agent.BeginPlan(plan);
    }

    public void ReplacePlan(AgentId agentId, Plan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var agent = GetAgent(agentId);
        if (agent.Plan is not null)
            FinishPlan(agentId, PlanOutcome.Cancelled);
        StartPlan(agentId, plan);
    }

    public void CancelPlan(AgentId agentId)
    {
        var agent = GetAgent(agentId);
        if (agent.Plan is not null)
            FinishPlan(agentId, PlanOutcome.Cancelled);
    }

    internal void FinishPlan(AgentId agentId, PlanOutcome outcome)
    {
        var agent = GetAgent(agentId);
        var plan = agent.Plan;
        if (plan is null) return;

        agent.Navigation.Reset();
        plan.InstructionTimeRemaining = 0f;
        TryReleaseItemReservationForAgent(agentId);

        if (TryGetOrderAssignmentForAgent(agentId, out _))
        {
            if (outcome == PlanOutcome.Succeeded)
                CompleteAssignedOrder(agentId);
            else
                ReleaseOrderAssignmentForAgent(agentId);
        }

        agent.ClearPlan();
    }
}
