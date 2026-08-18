using Godot;
using System;
using System.Collections.Generic;
using Solitude.Domain.Game.Items;
using Solitude.Domain.Game.Objects;

namespace Solitude.Domain.Game;

public readonly record struct ObjectDamageResult(
    Vector2I Cell,
    MapObjectType Type,
    bool Destroyed);

public sealed partial class World
{
    private int _nextObjectId = 1;
    private readonly Dictionary<MapObjectId, MapObject> _objects = new();
    private readonly Dictionary<Vector2I, MapObjectId> _objectByCell = new();

    public IReadOnlyCollection<MapObject> Objects => _objects.Values;

    public MapObjectId CreateObject(Vector2I cell, MapObject obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        if (!Grid.Contains(cell))
            throw new ArgumentOutOfRangeException(
                nameof(cell),
                cell,
                "Object cell is outside the grid.");
        if (obj.IsDestroyed)
            throw new ArgumentException(
                "A destroyed object cannot be added to the world.",
                nameof(obj));
        if (ObjectAt(cell) is not null)
            throw new InvalidOperationException($"Cell {cell} already contains an object.");
        if (ConstructionSiteAt(cell) is not null)
            throw new InvalidOperationException($"Cell {cell} contains a construction site.");

        var id = new MapObjectId(_nextObjectId++);
        obj.Id = id;
        obj.Cell = cell;
        _objects.Add(id, obj);
        _objectByCell.Add(cell, id);
        return id;
    }

    public bool TryGetObject(MapObjectId id, out MapObject obj) =>
        _objects.TryGetValue(id, out obj!);

    public MapObject GetObject(MapObjectId id)
    {
        if (!TryGetObject(id, out var obj))
            throw new KeyNotFoundException($"Object {id.Value} does not exist.");
        return obj;
    }

    public MapObject? ObjectAt(Vector2I cell) =>
        _objectByCell.TryGetValue(cell, out var id) && TryGetObject(id, out var obj)
            ? obj
            : null;

    public void RemoveObject(MapObjectId id)
    {
        var obj = GetObject(id);
        _objects.Remove(id);
        _objectByCell.Remove(obj.Cell);
        RemoveDamageOrderForTarget(id);
    }

    public ObjectDamageResult DamageObject(MapObjectId id, int amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));

        var obj = GetObject(id);
        if (obj.IsDestroyed)
            throw new InvalidOperationException($"Object {id.Value} is already destroyed.");

        obj.Damage(amount);
        var result = new ObjectDamageResult(obj.Cell, obj.Type, obj.IsDestroyed);
        if (!obj.IsDestroyed) return result;

        var drops = obj.GetDrops();
        RemoveObject(id);
        foreach (var drop in drops)
            CreateItem(obj.Cell, drop.Type, drop.Count);
        return result;
    }
}
