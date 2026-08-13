using System;
using System.Collections.Generic;
using Solitude.Domain.Game.Agents;
using Solitude.Domain.Game.Items;

namespace Solitude.Domain.Game;

public sealed partial class World
{
    private readonly ItemReservationLedger _itemReservations;

    public IReadOnlyCollection<ItemReservation> ItemReservations =>
        _itemReservations.Reservations;

    public bool TryGetItemReservationForAgent(
        AgentId agentId,
        out ItemReservation reservation)
    {
        GetAgent(agentId);
        var existing = _itemReservations.ForAgent(agentId);
        if (existing is null)
        {
            reservation = null!;
            return false;
        }
        reservation = existing;
        return true;
    }

    public ItemReservation GetItemReservationForAgent(AgentId agentId)
    {
        if (!TryGetItemReservationForAgent(agentId, out var reservation))
            throw new InvalidOperationException(
                $"Agent {agentId.Value} has no item reservation.");
        return reservation;
    }

    public int GetAvailableItemCount(ItemId itemId)
    {
        GetItem(itemId);
        return _itemReservations.GetAvailableCount(itemId);
    }

    public ItemReservationId ReserveItem(ItemId itemId, AgentId agentId, int count)
    {
        GetItem(itemId);
        GetAgent(agentId);
        return _itemReservations.Create(itemId, agentId, count);
    }

    public ItemStack ConsumeItemReservation(ItemReservationId reservationId, int maximumCount) =>
        _itemReservations.Consume(reservationId, maximumCount);

    public bool TryReleaseItemReservationForAgent(AgentId agentId)
    {
        if (!TryGetItemReservationForAgent(agentId, out _)) return false;
        _itemReservations.ReleaseForAgent(agentId);
        return true;
    }
}
