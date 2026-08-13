using Godot;
using System;
using Solitude.Domain.Game.Construction;
using Solitude.Domain.Game.Items;
using Solitude.Domain.Game.Map;
using Solitude.Domain.Game.Objects;

namespace Solitude.Domain.Game.Agents.Plans;

public abstract record PlanInstruction;

public abstract record MoveTarget;
public sealed record CellTarget(Vector2I Cell) : MoveTarget;
public sealed record ReservedItemTarget : MoveTarget;

public sealed record MoveInstruction : PlanInstruction
{
    public MoveTarget Target { get; }
    public PathGoalMode GoalMode { get; }

    public MoveInstruction(MoveTarget target, PathGoalMode goalMode)
    {
        Target = target ?? throw new ArgumentNullException(nameof(target));
        if (!Enum.IsDefined(goalMode))
            throw new ArgumentOutOfRangeException(nameof(goalMode), goalMode, null);
        GoalMode = goalMode;
    }
}

public sealed record HasItemCondition(ItemType Type) : PlanInstruction;
public sealed record ObjectExistsCondition(MapObjectId Target) : PlanInstruction;
public sealed record ConstructionSiteExistsCondition(
    ConstructionSiteId Target) : PlanInstruction;
public sealed record ConstructionRequirementSatisfiedCondition(
    ConstructionSiteId Target,
    ItemType Type) : PlanInstruction;

public sealed record ReserveItemInstruction : PlanInstruction
{
    public ItemType Type { get; }
    public int MaximumCount { get; }

    public ReserveItemInstruction(ItemType type, int maximumCount)
    {
        if (maximumCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(maximumCount));
        Type = type;
        MaximumCount = maximumCount;
    }
}

public sealed record CollectReservedItemInstruction : PlanInstruction;

public sealed record DamageObjectInstruction : PlanInstruction
{
    public MapObjectId Target { get; }
    public int Amount { get; }

    public DamageObjectInstruction(MapObjectId target, int amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        Target = target;
        Amount = amount;
    }
}

public sealed record SupplyConstructionInstruction(
    ConstructionSiteId Target,
    ItemType Type) : PlanInstruction;

public sealed record AdvanceConstructionInstruction : PlanInstruction
{
    public ConstructionSiteId Target { get; }
    public int Amount { get; }

    public AdvanceConstructionInstruction(ConstructionSiteId target, int amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        Target = target;
        Amount = amount;
    }
}

public sealed record WaitInstruction : PlanInstruction
{
    public float Duration { get; }

    public WaitInstruction(float duration)
    {
        if (!float.IsFinite(duration) || duration <= 0f)
            throw new ArgumentOutOfRangeException(nameof(duration));
        Duration = duration;
    }
}
