using Solitude.Domain.Game.Agents;

namespace Solitude.Domain.Game.Items;

public sealed class ItemReservation
{
	public required ItemReservationId Id { get; init; }
	public required ItemId ItemId { get; init; }
	public required AgentId AgentId { get; init; }
	public required int Count { get; init; }
}
