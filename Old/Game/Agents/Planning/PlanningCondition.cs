using System;
using Solitude.Domain.Game.Construction;
using Solitude.Domain.Game.Items;
using Solitude.Domain.Game.Objects;

namespace Solitude.Domain.Game.Agents.Planning;

public abstract record PlanningCondition;

public sealed record TirednessAtMostCondition : PlanningCondition
{
    public int Maximum { get; }

    public TirednessAtMostCondition(int maximum)
    {
        if (maximum < 0 || maximum > 100)
            throw new ArgumentOutOfRangeException(nameof(maximum));
        Maximum = maximum;
    }
}

public sealed record SatiationAtLeastCondition : PlanningCondition
{
    public int Minimum { get; }

    public SatiationAtLeastCondition(int minimum)
    {
        if (minimum < 0 || minimum > 100)
            throw new ArgumentOutOfRangeException(nameof(minimum));
        Minimum = minimum;
    }
}

public sealed record HasItemCondition : PlanningCondition
{
    public ItemType Type { get; }
    public int Count { get; }

    public HasItemCondition(ItemType type, int count)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
        Type = type;
        Count = count;
    }
}

public sealed record InventorySpaceAtLeastCondition : PlanningCondition
{
    public int Count { get; }

    public InventorySpaceAtLeastCondition(int count)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
        Count = count;
    }
}

public sealed record ItemCountAtLeastCondition : PlanningCondition
{
    public ItemId Target { get; }
    public int Count { get; }

    public ItemCountAtLeastCondition(ItemId target, int count)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
        Target = target;
        Count = count;
    }
}
public sealed record ObjectExistsCondition(MapObjectId Target) : PlanningCondition;
public sealed record ObjectAbsentCondition(MapObjectId Target) : PlanningCondition;
public sealed record ConstructionSiteExistsCondition(ConstructionSiteId Target)
    : PlanningCondition;
public sealed record ConstructionSiteAbsentCondition(ConstructionSiteId Target)
    : PlanningCondition;
public sealed record ConstructionMaterialRemainingAtLeastCondition
    : PlanningCondition
{
    public ConstructionSiteId Target { get; }
    public ItemType Type { get; }
    public int Count { get; }

    public ConstructionMaterialRemainingAtLeastCondition(
        ConstructionSiteId target,
        ItemType type,
        int count)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
        Target = target;
        Type = type;
        Count = count;
    }
}
public sealed record ConstructionFullySuppliedCondition(ConstructionSiteId Target)
    : PlanningCondition;
