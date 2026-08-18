using Godot;
using System;
using Solitude.Domain.Game.Construction;
using Solitude.Domain.Game.Items;
using Solitude.Domain.Game.Objects;

namespace Solitude.Domain.Game.Agents.Planning;

public abstract record PlanningEffect;

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

public sealed record RemoveAvailableItemEffect : PlanningEffect
{
    public ItemId Target { get; }
    public int Count { get; }

    public RemoveAvailableItemEffect(ItemId target, int count)
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
