using System;
using Solitude.Domain.Game.Agents;
using Solitude.Domain.Game.Agents.Plans;

namespace Solitude.Domain.Game;

public sealed partial class World
{
    public void StartPlan(AgentId agentId, Plan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (!plan.IsFresh)
            throw new ArgumentException("Only a fresh plan can be started.", nameof(plan));
        var agent = GetAgent(agentId);
        if (agent.Plan is not null)
            throw new InvalidOperationException(
                $"Agent {agent.Id.Value} already has a plan.");
        agent.BeginPlan(plan);
    }

    public void ReplacePlan(AgentId agentId, Plan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (!plan.IsFresh)
            throw new ArgumentException("Only a fresh plan can replace another plan.", nameof(plan));
        var agent = GetAgent(agentId);
        if (agent.Plan is not null)
            FinishPlan(agentId);
        StartPlan(agentId, plan);
    }

    public void CancelPlan(AgentId agentId)
    {
        var agent = GetAgent(agentId);
        if (agent.Plan is not null)
            FinishPlan(agentId);
    }

    internal void FinishPlan(AgentId agentId)
    {
        var agent = GetAgent(agentId);
        if (agent.Plan is null) return;

        agent.Navigation.Reset();
        agent.ClearPlan();
    }
}
