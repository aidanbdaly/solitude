using System;


public sealed class MapService(uint width, uint height)
{
    public event EventHandler<MapChangedEvent>? MapChanged;

    public event EventHandler<TimeChangedEvent>? TimeChanged;
    
    public void SetTime(uint newTime)
    {
        Time.Set(newTime);

        TimeChanged?.Invoke(this, new()
        {
            Time = newTime
        });
    }

    public uint GetTime() => Time.Get();

    public void SetTile(TileType type, Coordinate coordinate)
        => _tile.Set(coordinate, type);

    public TileType GetTile(Coordinate coordinate)
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

    public static Map FromDefinition(MapDefinition definition)
    {
        var map = new Map(definition.Width, definition.Height);

        var random = new Random(definition.Seed);

        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                var noise = PerlinNoise.Noise2DByte(
                    x * definition.NoiseScale,
                    y * definition.NoiseScale);

                var type = definition.Pallete[noise];

                map.SetTile(type, new(x, y));

                if (type == TileType.Stone)
                {
                    map.SetFeature(new(FeatureType.Rock), new(x, y));
                }

                if (type == TileType.Grass && random.NextSingle() < definition.FloraDensity)
                {
                    map.SetFeature(new(FeatureType.Flora), new(x, y));
                }
            }
        }

        return map;
    }
}
