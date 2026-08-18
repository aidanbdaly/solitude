using Solitude.Domain.Game.Agents;

namespace Solitude.Domain.Game;

public sealed partial class World
{
    public float IncreaseAgentTiredness(AgentId agentId, float amount) =>
        GetAgent(agentId).Needs.IncreaseTiredness(amount);

    public float DecreaseAgentTiredness(AgentId agentId, float amount) =>
        GetAgent(agentId).Needs.DecreaseTiredness(amount);

    public float IncreaseAgentSatiation(AgentId agentId, float amount) =>
        GetAgent(agentId).Needs.IncreaseSatiation(amount);

    public float DecreaseAgentSatiation(AgentId agentId, float amount) =>
        GetAgent(agentId).Needs.DecreaseSatiation(amount);
}
