using Godot;
using System;
using System.Collections.Immutable;
using System.Linq;
using Solitude.Domain.Game.Agents.Planning;
using Solitude.Domain.Game.Construction;
using Solitude.Domain.Game.Items;
using Solitude.Domain.Game.Map;
using Solitude.Domain.Game.Objects;

namespace Solitude.Domain.Game.Agents.Plans;

public abstract record PlanAction
{
    public ImmutableArray<PlanningCondition> Preconditions { get; }
    public ImmutableArray<PlanningEffect> Effects { get; }
    public float Cost { get; }

    protected PlanAction(
        ImmutableArray<PlanningCondition> preconditions,
        ImmutableArray<PlanningEffect> effects,
        float cost)
    {
        if (preconditions.IsDefault)
            throw new ArgumentException("Action preconditions must be initialized.", nameof(preconditions));
        if (effects.IsDefaultOrEmpty)
            throw new ArgumentException("An action requires at least one effect.", nameof(effects));
        if (preconditions.Any(condition => condition is null))
            throw new ArgumentException("Action preconditions cannot contain null.", nameof(preconditions));
        if (effects.Any(effect => effect is null))
            throw new ArgumentException("Action effects cannot contain null.", nameof(effects));
        if (!float.IsFinite(cost) || cost <= 0f)
            throw new ArgumentOutOfRangeException(nameof(cost));

        Preconditions = preconditions;
        Effects = effects;
        Cost = cost;
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
