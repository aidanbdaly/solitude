using System;
using System.Collections.Generic;
using System.Linq;
using Solitude.Domain.Game.Agents;
using Solitude.Domain.Game.Construction;
using Solitude.Domain.Game.Objects;
using Solitude.Domain.Game.Orders;

namespace Solitude.Domain.Game;

public sealed partial class World
{
    private readonly OrderBook _orders = new();
    private readonly OrderAssignments _orderAssignments = new();

    public IReadOnlyCollection<Order> Orders => _orders.Orders;
    public IReadOnlyCollection<OrderAssignment> OrderAssignments => _orderAssignments.Assignments;
    public IEnumerable<Order> UnassignedOrders =>
        _orders.Orders.Where(order => !_orderAssignments.IsAssigned(order.Id));

    public bool TryGetOrder(OrderId orderId, out Order order) =>
        _orders.TryGet(orderId, out order);

    public Order GetOrder(OrderId orderId) => _orders.Get(orderId);

    public bool IsOrderAssigned(OrderId orderId)
    {
        GetOrder(orderId);
        return _orderAssignments.IsAssigned(orderId);
    }

    public bool TryGetOrderAssignmentForAgent(
        AgentId agentId,
        out OrderAssignment assignment)
    {
        GetAgent(agentId);
        var existing = _orderAssignments.ForAgent(agentId);
        if (existing is null)
        {
            assignment = null!;
            return false;
        }
        assignment = existing;
        return true;
    }

    public OrderAssignment GetOrderAssignmentForAgent(AgentId agentId)
    {
        if (!TryGetOrderAssignmentForAgent(agentId, out var assignment))
            throw new InvalidOperationException(
                $"Agent {agentId.Value} has no order assignment.");
        return assignment;
    }

    public OrderId GetOrAddDamageOrder(MapObjectId target)
    {
        GetObject(target);
        return _orders.GetOrAddDamage(target);
    }

    public OrderId GetOrAddConstructOrder(ConstructionSiteId target)
    {
        GetConstructionSite(target);
        return _orders.GetOrAddConstruct(target);
    }

    public void AssignOrder(OrderId orderId, AgentId agentId)
    {
        GetOrder(orderId);
        GetAgent(agentId);
        _orderAssignments.Assign(orderId, agentId);
    }

    public void ReleaseOrderAssignmentForAgent(AgentId agentId)
    {
        var assignment = GetOrderAssignmentForAgent(agentId);
        _orderAssignments.Release(assignment.OrderId);
    }

    public void CompleteAssignedOrder(AgentId agentId)
    {
        var assignment = GetOrderAssignmentForAgent(agentId);
        GetOrder(assignment.OrderId);
        _orderAssignments.Release(assignment.OrderId);
        _orders.Remove(assignment.OrderId);
    }
}
