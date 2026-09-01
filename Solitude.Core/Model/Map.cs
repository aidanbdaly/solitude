using System;


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

    public IReadOnlyGrid<bool> Occupation => _occupation;

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

    public uint GetTime() => Time.Get();

    public void SetTile(Tile tile, Coordinate coordinate)
        => _tile.Set(coordinate, tile);

    public Tile GetTile(Coordinate coordinate)
        => _tile.Get(coordinate);

    public void SetFeature(Feature feature, Coordinate coordinate)
        => Occupy(_feature, coordinate, feature);

    public void RemoveFeature(Coordinate coordinate)
        => Release(_feature, coordinate);

    public void SetWork(Work work, Coordinate coordinate)
        => Occupy(_work, coordinate, work);

    public void RemoveWork(Coordinate coordinate)
        => Release(_work, coordinate);

    public void SetItem(Item item, Coordinate coordinate)
        => Occupy(_item, coordinate, item);

    public void RemoveItem(Item item, Coordinate coordinate)
    {
        if (!ReferenceEquals(_item.Get(coordinate), item))
        {
            throw new InvalidOperationException($"Item '{item.Id}' is not present at coordinate '{coordinate}'");
        }

        Release(_item, coordinate);
    }

    public void SetAgent(Agent agent, Coordinate coordinate)
        => Occupy(_agent, coordinate, agent);

    public void RemoveAgent(Agent agent, Coordinate coordinate)
    {
        if (!ReferenceEquals(_agent.Get(coordinate), agent))
        {
            throw new InvalidOperationException($"Agent '{agent.Id}' is not present at coordinate '{coordinate}'");
        }

        Release(_agent, coordinate);
    }

    public void SetItemAggregate(ItemAggregate aggregate, Coordinate coordinate)
        => Occupy(_aggregate, coordinate, aggregate);

    public void RemoveItemAggregate(Coordinate coordinate)
        => Release(_aggregate, coordinate);

    private void Occupy<T>(SparseGrid<T> entity, Coordinate coordinate, T value) where T : class
    {
        if (_occupation.Get(coordinate))
        {
            throw new InvalidOperationException($"Cannot place {typeof(T).AssemblyQualifiedName} : cell is occupied");
        }

        _occupation.Set(coordinate, true);
        entity.Set(coordinate, value);
    }

    private void Release<T>(SparseGrid<T> entity, Coordinate coordinate) where T : class
    {
        if (entity.Remove(coordinate))
        {
            _occupation.Set(coordinate, false);
        }
    }

    public Coordinate GetSize() => new(Width, Height);

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