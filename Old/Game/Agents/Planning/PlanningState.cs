using Godot;
using System.Collections.Immutable;
using System.Linq;
using Solitude.Domain.Game.Construction;
using Solitude.Domain.Game.Items;
using Solitude.Domain.Game.Objects;
using Solitude.Domain.Game.Orders;

namespace Solitude.Domain.Game.Agents.Planning;

public sealed record AgentPlanningState(
    AgentId Id,
    Vector2I Location,
    int Tiredness,
    int Satiation,
    int InventoryCapacity,
    float MovementTilesPerSecond,
    float DamageActionsPerSecond,
    float ConstructionActionsPerSecond,
    ImmutableDictionary<ItemType, int> Inventory)
{
    public int InventoryCount(ItemType type) => Inventory.TryGetValue(type, out var count)
        ? count
        : 0;

    public int InventoryAvailableCapacity =>
        InventoryCapacity - Inventory.Values.Sum();
}

public sealed record PlanningItem(
    ItemId Id,
    ItemType Type,
    Vector2I Cell,
    int Count);

public sealed record PlanningObject(
    MapObjectId Id,
    MapObjectType Type,
    Vector2I Cell,
    int HitPoints);

public sealed record PlanningConstructionSite(
    ConstructionSiteId Id,
    BuildingType BuildingType,
    Vector2I Cell,
    int ProgressRemaining,
    ImmutableDictionary<ItemType, int> MissingMaterials)
{
    public bool IsFullySupplied => MissingMaterials.Values.All(count => count == 0);

    public int MissingCount(ItemType type) =>
        MissingMaterials.TryGetValue(type, out var count) ? count : 0;
}

public abstract record PlanningOrder(OrderId Id);
public sealed record PlanningDamageOrder(OrderId Id, MapObjectId Target)
    : PlanningOrder(Id);
public sealed record PlanningConstructOrder(OrderId Id, ConstructionSiteId Target)
    : PlanningOrder(Id);

public sealed record PlanningState(
    AgentPlanningState Agent,
    ImmutableDictionary<ItemId, PlanningItem> Items,
    ImmutableDictionary<MapObjectId, PlanningObject> Objects,
    ImmutableDictionary<ConstructionSiteId, PlanningConstructionSite> ConstructionSites,
    ImmutableArray<PlanningOrder> Orders,
    PlanningNavigationMap Navigation)
{
    public PlanningStateKey CreateKey() => PlanningStateKey.From(this);
}
