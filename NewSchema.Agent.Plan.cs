using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;



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




public sealed record SetAgentLocationEffect(Vector2I Cell) : PlanningEffect;
public sealed record AddAgentItemEffect : PlanningEffect
{
    public ItemType Type { get; }
    public int Count { get; }

    public AddAgentItemEffect(ItemType type, int count)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
        Type = type;
        Count = count;
    }
}

public sealed record RemoveAgentItemEffect : PlanningEffect
{
    public ItemType Type { get; }
    public int Count { get; }

    public RemoveAgentItemEffect(ItemType type, int count)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
        Type = type;
        Count = count;
    }
}

public sealed record DecreaseItemCountEffect : PlanningEffect
{
    public ItemId Target { get; }
    public int Count { get; }

    public DecreaseItemCountEffect(ItemId target, int count)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
        Target = target;
        Count = count;
    }
}

public sealed record SupplyConstructionMaterialEffect : PlanningEffect
{
    public ConstructionSiteId Target { get; }
    public ItemType Type { get; }
    public int Count { get; }

    public SupplyConstructionMaterialEffect(
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
public sealed record RemoveObjectEffect(MapObjectId Target) : PlanningEffect;
public sealed record CompleteConstructionEffect(ConstructionSiteId Target) : PlanningEffect;
public sealed record IncreaseTirednessEffect : PlanningEffect
{
    public int Amount { get; }
    public IncreaseTirednessEffect(int amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        Amount = amount;
    }
}

public sealed record DecreaseTirednessEffect : PlanningEffect
{
    public int Amount { get; }
    public DecreaseTirednessEffect(int amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        Amount = amount;
    }
}

public sealed record IncreaseSatiationEffect : PlanningEffect
{
    public int Amount { get; }
    public IncreaseSatiationEffect(int amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        Amount = amount;
    }
}

public sealed record DecreaseSatiationEffect : PlanningEffect
{
    public int Amount { get; }
    public DecreaseSatiationEffect(int amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        Amount = amount;
    }
}




public sealed record MoveToAction : PlanAction
{
    public Vector2I Target { get; }

    public MoveToAction(Vector2I target)
        : base(
            ImmutableArray<PlanningCondition>.Empty,
            ImmutableArray.Create<PlanningEffect>(new SetAgentLocationEffect(target)),
            1f)
    {
        Target = target;
    }
}

public sealed record CollectItemAction : PlanAction
{
    public ItemId Target { get; }
    public ItemType Type { get; }
    public int Count { get; }
    public Vector2I Cell { get; }

    public CollectItemAction(
        ItemId target,
        ItemType type,
        int count,
        Vector2I cell,
        float cost)
        : base(
            ImmutableArray.Create<PlanningCondition>(
                new ItemCountAtLeastCondition(target, count),
                new InventorySpaceAtLeastCondition(count)),
            ImmutableArray.Create<PlanningEffect>(
                new SetAgentLocationEffect(cell),
                new DecreaseItemCountEffect(target, count),
                new AddAgentItemEffect(type, count)),
            cost)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
        Target = target;
        Type = type;
        Count = count;
        Cell = cell;
    }
}

public sealed record SupplyConstructionAction : PlanAction
{
    public ConstructionSiteId Target { get; }
    public ItemType Type { get; }
    public int Count { get; }
    public Vector2I TargetCell { get; }
    public Vector2I Destination { get; }

    public SupplyConstructionAction(
        ConstructionSiteId target,
        ItemType type,
        int count,
        Vector2I targetCell,
        Vector2I destination,
        float cost)
        : base(
            ImmutableArray.Create<PlanningCondition>(
                new ConstructionSiteExistsCondition(target),
                new HasItemCondition(type, count),
                new ConstructionMaterialRemainingAtLeastCondition(target, type, count)),
            ImmutableArray.Create<PlanningEffect>(
                new SetAgentLocationEffect(destination),
                new RemoveAgentItemEffect(type, count),
                new SupplyConstructionMaterialEffect(target, type, count)),
            cost)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
        if (!GridMath.IsAdjacent(destination, targetCell))
            throw new ArgumentException(
                "A construction supply destination must be adjacent to its site.",
                nameof(destination));
        Target = target;
        Type = type;
        Count = count;
        TargetCell = targetCell;
        Destination = destination;
    }
}

public sealed record DamageObjectAction : PlanAction
{
    public MapObjectId Target { get; }
    public Vector2I TargetCell { get; }
    public Vector2I Destination { get; }

    public DamageObjectAction(
        MapObjectId target,
        Vector2I targetCell,
        Vector2I destination,
        float cost)
        : base(
            ImmutableArray.Create<PlanningCondition>(new ObjectExistsCondition(target)),
            ImmutableArray.Create<PlanningEffect>(
                new SetAgentLocationEffect(destination),
                new RemoveObjectEffect(target)),
            cost)
    {
        if (!GridMath.IsAdjacent(destination, targetCell))
            throw new ArgumentException(
                "A damage destination must be adjacent to its target.",
                nameof(destination));
        Target = target;
        TargetCell = targetCell;
        Destination = destination;
    }
}

public sealed record ConstructAction : PlanAction
{
    public ConstructionSiteId Target { get; }
    public Vector2I TargetCell { get; }
    public Vector2I Destination { get; }

    public ConstructAction(
        ConstructionSiteId target,
        Vector2I targetCell,
        Vector2I destination,
        float cost)
        : base(
            ImmutableArray.Create<PlanningCondition>(
                new ConstructionSiteExistsCondition(target),
                new ConstructionFullySuppliedCondition(target)),
            ImmutableArray.Create<PlanningEffect>(
                new SetAgentLocationEffect(destination),
                new CompleteConstructionEffect(target)),
            cost)
    {
        if (!GridMath.IsAdjacent(destination, targetCell))
            throw new ArgumentException(
                "A construction destination must be adjacent to its site.",
                nameof(destination));
        Target = target;
        TargetCell = targetCell;
        Destination = destination;
    }
}


