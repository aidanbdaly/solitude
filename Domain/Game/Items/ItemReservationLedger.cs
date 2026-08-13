using System;
using System.Collections.Generic;
using System.Linq;
using Solitude.Domain.Game.Agents;

namespace Solitude.Domain.Game.Items;

internal sealed class ItemReservationLedger
{
    private int _nextId = 1;
    private readonly IReadOnlyDictionary<ItemId, Item> _items;
    private readonly Dictionary<ItemReservationId, ItemReservation> _reservations = new();

    internal IReadOnlyCollection<ItemReservation> Reservations => _reservations.Values;

    internal ItemReservationLedger(IReadOnlyDictionary<ItemId, Item> items)
    {
        _items = items;
    }

    internal ItemReservationId Create(ItemId itemId, AgentId agentId, int count)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
        if (!_items.ContainsKey(itemId))
            throw new KeyNotFoundException($"Item {itemId.Value} does not exist.");
        if (_reservations.Values.Any(reservation => reservation.AgentId == agentId))
            throw new InvalidOperationException($"Agent {agentId.Value} already has an item reservation.");
        if (count > GetAvailableCount(itemId))
            throw new InvalidOperationException(
                $"Item {itemId.Value} does not have {count} available units.");

        var id = new ItemReservationId(_nextId++);
        _reservations.Add(id, new ItemReservation
        {
            Id = id,
            ItemId = itemId,
            AgentId = agentId,
            Count = count
        });
        return id;
    }

    internal ItemReservation? ForAgent(AgentId agentId) =>
        _reservations.Values.FirstOrDefault(reservation => reservation.AgentId == agentId);

    internal int GetAvailableCount(ItemId itemId) => _items.TryGetValue(itemId, out var item)
        ? Math.Max(0, item.Count - _reservations.Values
            .Where(reservation => reservation.ItemId == itemId)
            .Sum(reservation => reservation.Count))
        : 0;

    internal ItemStack Consume(ItemReservationId id, int maximumCount)
    {
        if (maximumCount <= 0) throw new ArgumentOutOfRangeException(nameof(maximumCount));
        if (!_reservations.TryGetValue(id, out var reservation))
            throw new KeyNotFoundException($"Item reservation {id.Value} does not exist.");
        if (!_items.TryGetValue(reservation.ItemId, out var item))
            throw new InvalidOperationException(
                $"Reserved item {reservation.ItemId.Value} does not exist.");
        var count = Math.Min(reservation.Count, Math.Max(0, maximumCount));
        if (count <= 0)
            throw new InvalidOperationException($"Item reservation {id.Value} has no consumable units.");
        var stack = item.Take(count);
        if (stack.Count != count)
            throw new InvalidOperationException(
                $"Reserved item {reservation.ItemId.Value} contained fewer units than reserved.");
        _reservations.Remove(id);
        return stack;
    }

    internal void ReleaseForAgent(AgentId agentId)
    {
        foreach (var id in _reservations
                     .Where(pair => pair.Value.AgentId == agentId)
                     .Select(pair => pair.Key)
                     .ToArray())
            _reservations.Remove(id);
    }

    internal void ReleaseForItem(ItemId itemId)
    {
        foreach (var id in _reservations
                     .Where(pair => pair.Value.ItemId == itemId)
                     .Select(pair => pair.Key)
                     .ToArray())
            _reservations.Remove(id);
    }
}
