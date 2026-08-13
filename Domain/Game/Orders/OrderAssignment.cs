using Solitude.Domain.Game.Agents;

namespace Solitude.Domain.Game.Orders;

public sealed class OrderAssignment
{
    public required OrderId OrderId { get; init; }
    public required AgentId AgentId { get; init; }
}
