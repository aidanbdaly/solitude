using Godot;
using System;
using Solitude.Domain.Game.Construction;
using Solitude.Domain.Game.Items;
using Solitude.Domain.Game.Map;
using Solitude.Domain.Game.Objects;

namespace Solitude.Domain.Game.Agents.Activity;

public abstract record ActivityInstruction;

public abstract record MoveTarget;
public sealed record CellTarget(Vector2I Cell) : MoveTarget;
public sealed record ReservedItemTarget : MoveTarget;

public sealed record MoveInstruction : ActivityInstruction
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

public sealed record HasItemCondition(ItemType Type) : ActivityInstruction;
public sealed record ObjectExistsCondition(MapObjectId Target) : ActivityInstruction;
public sealed record ConstructionSiteExistsCondition(
    ConstructionSiteId Target) : ActivityInstruction;
public sealed record ConstructionRequirementSatisfiedCondition(
    ConstructionSiteId Target,
    ItemType Type) : ActivityInstruction;

public sealed record ReserveItemInstruction : ActivityInstruction
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

public sealed record CollectReservedItemInstruction : ActivityInstruction;

public sealed record DamageObjectInstruction : ActivityInstruction
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
    ItemType Type) : ActivityInstruction;

public sealed record AdvanceConstructionInstruction : ActivityInstruction
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

public sealed record WaitInstruction : ActivityInstruction
{
    public float Duration { get; }

    public WaitInstruction(float duration)
    {
        if (!float.IsFinite(duration) || duration <= 0f)
            throw new ArgumentOutOfRangeException(nameof(duration));
        Duration = duration;
    }
}
