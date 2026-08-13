using System;
using System.Collections.Generic;
using Solitude.Domain.Game.Agents;

namespace Solitude.Domain.Game.Orders;

internal sealed class OrderAssignments
{
    private readonly Dictionary<OrderId, OrderAssignment> _assignments = new();
    private readonly Dictionary<AgentId, OrderId> _orderByAgent = new();

    internal IReadOnlyCollection<OrderAssignment> Assignments => _assignments.Values;

    internal bool IsAssigned(OrderId orderId) => _assignments.ContainsKey(orderId);

    internal OrderAssignment? ForAgent(AgentId agentId) =>
        _orderByAgent.TryGetValue(agentId, out var orderId) ? _assignments[orderId] : null;

    internal void Assign(OrderId orderId, AgentId agentId)
    {
        if (_assignments.ContainsKey(orderId))
            throw new InvalidOperationException($"Order {orderId.Value} is already assigned.");
        if (_orderByAgent.ContainsKey(agentId))
            throw new InvalidOperationException($"Agent {agentId.Value} already has an order assignment.");

        _assignments.Add(orderId, new OrderAssignment { OrderId = orderId, AgentId = agentId });
        _orderByAgent.Add(agentId, orderId);
    }

    internal void Release(OrderId orderId)
    {
        if (!_assignments.Remove(orderId, out var assignment))
            throw new KeyNotFoundException($"Order {orderId.Value} has no assignment.");
        _orderByAgent.Remove(assignment.AgentId);
    }

}
