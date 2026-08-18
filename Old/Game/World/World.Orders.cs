using System.Collections.Generic;
using Solitude.Domain.Game.Construction;
using Solitude.Domain.Game.Objects;
using Solitude.Domain.Game.Orders;

namespace Solitude.Domain.Game;

public sealed partial class World
{
    private readonly OrderBook _orders = new();

    public IReadOnlyCollection<Order> Orders => _orders.Orders;

    public bool TryGetOrder(OrderId orderId, out Order order) =>
        _orders.TryGet(orderId, out order);

    public Order GetOrder(OrderId orderId) => _orders.Get(orderId);

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

    private void RemoveDamageOrderForTarget(MapObjectId target) =>
        _orders.RemoveDamageForTarget(target);

    private void RemoveConstructOrderForTarget(ConstructionSiteId target) =>
        _orders.RemoveConstructForTarget(target);
}
