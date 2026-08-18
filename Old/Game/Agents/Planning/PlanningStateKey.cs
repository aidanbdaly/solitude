using Godot;
using System;
using System.Collections.Immutable;
using System.Linq;
using Solitude.Domain.Game.Construction;
using Solitude.Domain.Game.Items;

namespace Solitude.Domain.Game.Agents.Planning;

public sealed class PlanningStateKey : IEquatable<PlanningStateKey>
{
    private readonly AgentId _agentId;
    private readonly Vector2I _location;
    private readonly int _tiredness;
    private readonly int _satiation;
    private readonly ImmutableArray<InventoryEntry> _inventory;
    private readonly ImmutableArray<ItemEntry> _items;
    private readonly ImmutableArray<ObjectEntry> _objects;
    private readonly ImmutableArray<ConstructionSiteEntry> _constructionSites;
    private readonly ImmutableArray<ConstructionMaterialEntry> _constructionMaterials;

    private PlanningStateKey(PlanningState state)
    {
        _agentId = state.Agent.Id;
        _location = state.Agent.Location;
        _tiredness = state.Agent.Tiredness;
        _satiation = state.Agent.Satiation;
        _inventory = state.Agent.Inventory
            .OrderBy(pair => pair.Key)
            .Select(pair => new InventoryEntry(pair.Key, pair.Value))
            .ToImmutableArray();
        _items = state.Items.Values
            .OrderBy(item => item.Id.Value)
            .Select(item => new ItemEntry(item.Id.Value, item.Count))
            .ToImmutableArray();
        _objects = state.Objects.Values
            .OrderBy(obj => obj.Id.Value)
            .Select(obj => new ObjectEntry(obj.Id.Value, obj.HitPoints))
            .ToImmutableArray();
        _constructionSites = state.ConstructionSites.Values
            .OrderBy(site => site.Id.Value)
            .Select(site => new ConstructionSiteEntry(site.Id.Value, site.ProgressRemaining))
            .ToImmutableArray();
        _constructionMaterials = state.ConstructionSites.Values
            .SelectMany(site => site.MissingMaterials.Select(pair =>
                new ConstructionMaterialEntry(site.Id, pair.Key, pair.Value)))
            .OrderBy(entry => entry.Site.Value)
            .ThenBy(entry => entry.Type)
            .ToImmutableArray();
    }

    public static PlanningStateKey From(PlanningState state) =>
        new(state ?? throw new ArgumentNullException(nameof(state)));

    public bool Equals(PlanningStateKey? other) =>
        other is not null
        && _agentId == other._agentId
        && _location == other._location
        && _tiredness == other._tiredness
        && _satiation == other._satiation
        && _inventory.SequenceEqual(other._inventory)
        && _items.SequenceEqual(other._items)
        && _objects.SequenceEqual(other._objects)
        && _constructionSites.SequenceEqual(other._constructionSites)
        && _constructionMaterials.SequenceEqual(other._constructionMaterials);

    public override bool Equals(object? obj) =>
        obj is PlanningStateKey other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_agentId);
        hash.Add(_location);
        hash.Add(_tiredness);
        hash.Add(_satiation);
        AddRange(ref hash, _inventory);
        AddRange(ref hash, _items);
        AddRange(ref hash, _objects);
        AddRange(ref hash, _constructionSites);
        AddRange(ref hash, _constructionMaterials);
        return hash.ToHashCode();
    }

    private static void AddRange<T>(ref HashCode hash, ImmutableArray<T> values)
    {
        hash.Add(values.Length);
        foreach (var value in values) hash.Add(value);
    }

    private readonly record struct InventoryEntry(ItemType Type, int Count);
    private readonly record struct ItemEntry(int Id, int Count);
    private readonly record struct ObjectEntry(int Id, int HitPoints);
    private readonly record struct ConstructionSiteEntry(int Id, int ProgressRemaining);
    private readonly record struct ConstructionMaterialEntry(
        ConstructionSiteId Site,
        ItemType Type,
        int MissingCount);
}
