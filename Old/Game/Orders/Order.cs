using Solitude.Domain.Game.Construction;
using Solitude.Domain.Game.Objects;

namespace Solitude.Domain.Game.Orders;

public abstract record Order
{
    public required OrderId Id { get; init; }
}

public sealed record DamageOrder : Order
{
    public required MapObjectId Target { get; init; }
}

public sealed record ConstructOrder : Order
{
    public required ConstructionSiteId Target { get; init; }
}
