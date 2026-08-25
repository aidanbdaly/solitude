using System;
using System.Collections.Generic;
using Godot;

public interface IReadOnlyStaticMapEntity<T>
{
    T Get(Vector2I coordinate);
    IEnumerator<(Vector2I coordinate, T value)> GetEnumerator();
}

public sealed class StaticMapEntity<T>(uint width, uint height) : IReadOnlyStaticMapEntity<T> where T : struct
{
    private readonly T[] _cells =
        new T[checked((int)((ulong)width * height))];

    public void Set(Vector2I coordinate, T value)
    {
        _cells[GetCellId(coordinate)] = value;
    }

    public T Get(Vector2I coordinate)
    {
        return _cells[GetCellId(coordinate)];
    }

    public IEnumerator<(Vector2I coordinate, T value)> GetEnumerator()
    {
        var i = 0;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                yield return (new Vector2I(x, y), _cells[i++]);
            }
        }
    }

    private int GetCellId(Vector2I coordinate)
    {
        if ((uint)coordinate.X >= width ||
            (uint)coordinate.Y >= height)
        {
            throw new ArgumentOutOfRangeException(nameof(coordinate));
        }

        return checked((int)((ulong)(uint)coordinate.Y * width +
                                 (uint)coordinate.X));
    }
}

public interface IReadOnlyMapEntity<T>
{
    T? Get(Vector2I coordinate);
    IEnumerator<(Vector2I coordinate, T value)> GetEnumerator();
}

public sealed class MapEntity<T>(uint width, uint height) : IReadOnlyMapEntity<T> where T : class
{
    private readonly T?[] _cells =
        new T?[checked((int)((ulong)width * height))];

    private readonly Dictionary<T, int> _entityToCell = [];

    public void Set(Vector2I coordinate, T value)
    {
        var cellId = GetCellId(coordinate);

        if (_cells[cellId] is not null)
        {
            throw new InvalidOperationException("Cannot set entity coordinate, cell is already occupied");
        }

        if (_entityToCell.TryGetValue(value, out var currentCellId))
        {
            _cells[currentCellId] = null;
        }

        _cells[cellId] = value;
        _entityToCell[value] = cellId;
    }

    public IEnumerator<(Vector2I coordinate, T value)> GetEnumerator()
    {
        var i = 0;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var value = _cells[i++];

                if (value is not null)
                {
                    yield return (new Vector2I(x, y), value);
                }
            }
        }
    }

    public T? Get(Vector2I coordinate)
    {
        return _cells[GetCellId(coordinate)];
    }

    public bool Remove(Vector2I coordinate)
    {
        var cellId = GetCellId(coordinate);

        var entity = _cells[cellId];

        if (entity is null)
        {
            return false;
        }

        _entityToCell.Remove(entity);
        _cells[cellId] = null;

        return true;
    }

    private int GetCellId(Vector2I coordinate)
    {
        if ((uint)coordinate.X >= width ||
            (uint)coordinate.Y >= height)
        {
            throw new ArgumentOutOfRangeException(nameof(coordinate));
        }

        return checked((int)((ulong)(uint)coordinate.Y * width +
                                 (uint)coordinate.X));
    }
}

public sealed class MapState(uint width, uint height)
{
    public int Width { get; } = checked((int)width);

    public int Height { get; } = checked((int)height);

    private readonly StaticMapEntity<bool> _occupation = new(width, height);

    private readonly StaticMapEntity<Tile> _tile = new(width, height);

    private readonly MapEntity<Feature> _feature = new(width, height);

    private readonly MapEntity<Work> _work = new(width, height);

    private readonly MapEntity<Item> _item = new(width, height);

    private readonly MapEntity<ItemAggregate> _aggregate = new(width, height);

    private readonly MapEntity<Agent> _agent = new(width, height);

    public IReadOnlyStaticMapEntity<Tile> Tile => _tile;

    public IReadOnlyMapEntity<Feature> Feature => _feature;

    public IReadOnlyMapEntity<Work> Work => _work;

    public IReadOnlyMapEntity<Item> Item => _item;
    
    public IReadOnlyMapEntity<Agent> Agent => _agent;

    public void SetTile(Vector2I coordinate, Tile tile)
        => _tile.Set(coordinate, tile);

    public Tile GetTile(Vector2I coordinate)
        => _tile.Get(coordinate);

    public void SetFeature(Vector2I coordinate, Feature feature)
        => Occupy(coordinate, _feature, feature);

    public void RemoveFeature(Vector2I coordinate)
        => Release(coordinate, _feature);

    public void SetWork(Vector2I coordinate, Work work)
        => Occupy(coordinate, _work, work);

    public void RemoveWork(Vector2I coordinate)
        => Release(coordinate, _work);

    public void SetItem(Vector2I coordinate, Item item)
        => Occupy(coordinate, _item, item);

    public void RemoveItem(Vector2I coordinate)
        => Release(coordinate, _item);

    public void SetAgent(Vector2I coordinate, Agent agent)
        => Occupy(coordinate, _agent, agent);

    public void RemoveAgent(Vector2I coordinate)
        => Release(coordinate, _agent);

    private void Occupy<T>(Vector2I coordinate, MapEntity<T> entity, T value) where T : class
    {
        if (_occupation.Get(coordinate))
        {
            throw new InvalidOperationException("Cannot place featurePlan: cell is occupied");
        }

        _occupation.Set(coordinate, true);
        entity.Set(coordinate, value);
    }

    private void Release<T>(Vector2I coordinate, MapEntity<T> entity) where T : class
    {
        if (entity.Remove(coordinate))
        {
            _occupation.Set(coordinate, false);
        }
    }

    public Vector2 GetSize()
    {
        return new Vector2(
            Width,
            Height
        );
    }
}