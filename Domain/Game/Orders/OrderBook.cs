using System.Collections.Generic;
using System.Linq;
using Solitude.Domain.Game.Construction;
using Solitude.Domain.Game.Objects;

namespace Solitude.Domain.Game.Orders;

internal sealed class OrderBook
{
    private int _nextId = 1;
    private readonly Dictionary<OrderId, Order> _orders = new();

    internal IReadOnlyCollection<Order> Orders => _orders.Values;

    internal bool TryGet(OrderId id, out Order order) =>
        _orders.TryGetValue(id, out order!);

    internal Order Get(OrderId id) =>
        TryGet(id, out var order)
            ? order
            : throw new KeyNotFoundException($"Order {id.Value} does not exist.");

    internal OrderId GetOrAddDamage(MapObjectId target)
    {
        var existing = _orders.Values.OfType<DamageOrder>().FirstOrDefault(order => order.Target == target);
        if (existing is not null) return existing.Id;

        var id = NextId();
        _orders.Add(id, new DamageOrder { Id = id, Target = target });
        return id;
    }

    internal OrderId GetOrAddConstruct(ConstructionSiteId target)
    {
        var existing = _orders.Values.OfType<ConstructOrder>().FirstOrDefault(order => order.Target == target);
        if (existing is not null) return existing.Id;

        var id = NextId();
        _orders.Add(id, new ConstructOrder { Id = id, Target = target });
        return id;
    }

    internal void Remove(OrderId id)
    {
        if (!_orders.Remove(id))
            throw new KeyNotFoundException($"Order {id.Value} does not exist.");
    }

    private OrderId NextId() => new(_nextId++);
}
