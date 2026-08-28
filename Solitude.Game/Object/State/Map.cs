using System;
using Godot;

public sealed class TimeChangedEvent : EventArgs
{
    public required uint Time { get; init; }
};

public sealed class Map(uint width, uint height)
{
    public event EventHandler<TimeChangedEvent>? TimeChanged;

    public int Width { get; } = checked((int)width);

    public int Height { get; } = checked((int)height);

    private readonly Time Time = new();

    private readonly Grid<bool> _occupation = new(width, height);

    private readonly Grid<Tile> _tile = new(width, height);

    private readonly SparseGrid<Feature> _feature = new(width, height);

    private readonly SparseGrid<Work> _work = new(width, height);

    private readonly SparseGrid<Item> _item = new(width, height);

    private readonly SparseGrid<ItemAggregate> _aggregate = new(width, height);

    private readonly SparseGrid<Agent> _agent = new(width, height);

    private readonly Vector2I?[] _agentMapCoordinate = new Vector2I?[checked((int)((ulong)width * height))];

    public IReadOnlyGrid<Tile> Tile => _tile;

    public IReadOnlySparseGrid<Feature> Feature => _feature;

    public IReadOnlySparseGrid<Work> Work => _work;

    public IReadOnlySparseGrid<Item> Item => _item;

    public IReadOnlySparseGrid<ItemAggregate> Aggregate => _aggregate;

    public IReadOnlySparseGrid<Agent> Agent => _agent;

    public void SetTime(uint newTime)
    {
        Time.Set(newTime);

        TimeChanged?.Invoke(this, new()
        {
            Time = newTime
        });
    }

    public void SetTile(Tile tile, Vector2I coordinate)
        => _tile.Set(coordinate, tile);

    public Tile GetTile(Vector2I coordinate)
        => _tile.Get(coordinate);

    public void SetFeature(Feature feature, Vector2I coordinate)
        => Occupy(_feature, coordinate, feature);

    public void RemoveFeature(Vector2I coordinate)
        => Release(_feature, coordinate);

    public void SetWork(Work work, Vector2I coordinate)
        => Occupy(_work, coordinate, work);

    public void RemoveWork(Vector2I coordinate)
        => Release(_work, coordinate);

    public void SetItem(Item item, Vector2I coordinate)
        => Occupy(_item, coordinate, item);

    public void RemoveItem(Vector2I coordinate)
        => Release(_item, coordinate);

    public void SetAgent(Agent agent, Vector2I coordinate)
    {
        Occupy(_agent, coordinate, agent);
        _agentMapCoordinate[agent.Id] = coordinate;
    }

    public void RemoveAgent(Agent agent)
    {
        if (_agentMapCoordinate[agent.Id] is Vector2I coordinate)
        {
            Release(_agent, coordinate);
            _agentMapCoordinate[agent.Id] = null;
        }
        else
        {
            throw new InvalidOperationException("Call to RemoveAgent() failed: Agent is not present on map");
        }
    }

    public void SetItemAggregate(ItemAggregate aggregate, Vector2I coordinate)
        => Occupy(_aggregate, coordinate, aggregate);

    public void RemoveItemAggregate(Vector2I coordinate)
        => Release(_aggregate, coordinate);

    private void Occupy<T>(SparseGrid<T> entity, Vector2I coordinate, T value) where T : class
    {
        if (_occupation.Get(coordinate))
        {
            throw new InvalidOperationException("Cannot place featurePlan: cell is occupied");
        }

        _occupation.Set(coordinate, true);
        entity.Set(coordinate, value);
    }

    private void Release<T>(SparseGrid<T> entity, Vector2I coordinate) where T : class
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

    public static Map Generate(MapStyle style)
    {
        var map = new Map(style.Width, style.Height);

        var generation = style.Generation;

        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                var noise = PerlinNoise.Noise2D(
                    x * generation.NoiseScale,
                    y * generation.NoiseScale);

                var type = noise < generation.WaterThreshold
                    ? TileType.Water
                    : noise < generation.GrassThreshold ? TileType.Grass : TileType.Stone;

                map.SetTile(new(type), new(x, y));

                if (type == TileType.Stone)
                {
                    map.SetFeature(new(FeatureType.Rock), new(x, y));
                }

                if (type == TileType.Grass)
                {
                    map.SetFeature(new(FeatureType.Flora), new(x, y)); // Spread out
                }
            }
        }

        return map;
    }
}