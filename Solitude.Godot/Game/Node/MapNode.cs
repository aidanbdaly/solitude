using System;
using Godot;

public partial class MapNode : Node2D
{
    private const int SourceTileSize = 16;

    [Export] public Texture2D TileAtlas { get; set; } = null!;
    [Export] public Texture2D FeatureAtlas { get; set; } = null!;
    [Export] public Texture2D ItemAtlas { get; set; } = null!;
    [Export] public Texture2D AgentAtlas { get; set; } = null!;

    private Map? _map = null;
    private Texture2D? _cache = null;

    private bool _cacheStale = true;
    private float _simulationSpeed = 0f;
    private float _lastSimulationSpeed = 1f;

    [Signal] public delegate void TimeChangedEventHandler(int newTime);

    public override void _Ready()
    {
        TextureFilter = TextureFilterEnum.Nearest;
    }

    public override void _ExitTree()
    {
        if (_map is not null)
        {
            _map.Tile.GridChanged -= OnGridChanged;
            _map.TimeChanged -= OnTimeChanged;
        }
    }

    public void Bind(Map map)
    {
        if (!IsNodeReady())
        {
            throw new InvalidOperationException(
                "MapNode must be added to the scene tree before binding.");
        }

        if (_map is not null)
        {
            _map.Tile.GridChanged -= OnGridChanged;
            _map.TimeChanged -= OnTimeChanged;
        }

        _map = map;
        _map.Tile.GridChanged += OnGridChanged;
        _map.TimeChanged += OnTimeChanged;

        _cacheStale = true;
        GetNode<Camera>("Camera").SetSurface(ToVector2(map.GetSize()) * SourceTileSize);
        QueueRedraw();
    }

    private void OnGridChanged(object? e, GridChangedEvent evt)
        => _cacheStale = true;

    private void OnTimeChanged(object? e, TimeChangedEvent evt)
        => EmitSignal(SignalName.TimeChanged, evt.Time);

    public void SetSimulationSpeed(float speed)
    {
        _lastSimulationSpeed = _simulationSpeed;
        _simulationSpeed = speed;
    }

    public override void _Process(double delta)
    {
        if (_map is null)
        {
            return;
        }

        if (_cacheStale)
        {
            _cache = CacheTile(_map, TileAtlas.GetImage());
            _cacheStale = false;
            QueueRedraw();
        }
    }

    private static ImageTexture CacheTile(Map map, Image atlas)
    {
        var cache = Image.CreateEmpty(
            map.Width * SourceTileSize,
            map.Height * SourceTileSize,
            false,
            atlas.GetFormat()
        );

        foreach (var (coordinate, tile) in map.Tile)
        {
            int srcX;
            int srcY = (int)tile * SourceTileSize;

            if (coordinate.Y != 0)
            {
                srcX = (map.GetTile(new(coordinate.X, coordinate.Y - 1)) == tile ? 1 : 0) * SourceTileSize;
            }
            else
            {
                srcX = 0;
            }

            cache.BlitRect(
                atlas,
                new Rect2I(
                    srcX,
                    srcY,
                    SourceTileSize,
                    SourceTileSize),
                ToVector2I(coordinate) * SourceTileSize);
        }

        return ImageTexture.CreateFromImage(cache);
    }

    private void DrawCache(Map map)
        => DrawTextureRect(_cache, new Rect2(Vector2.Zero, ToVector2(map.GetSize()) * SourceTileSize), false);

    private void DrawItemAggregate(Map map)
    {
        foreach (var (coordinate, value) in map.Aggregate)
        {
            var origin = new Vector2(
                    coordinate.X * SourceTileSize,
                    coordinate.Y * SourceTileSize);

            DrawRect(new Rect2(origin, new Vector2(SourceTileSize, SourceTileSize)), new Color(0f, 0f, 0f, 0.5f));
        }
    }

    private void DrawWork(Map map)
    {
        foreach (var (coordinate, value) in map.Work)
        {
            var backColor = new Color(0.08f, 0.1f, 0.12f, 0.85f);
            var foreColor = new Color(0.92f, 0.72f, 0.3f);

            var origin = new Vector2(
                    coordinate.X * SourceTileSize,
                    coordinate.Y * SourceTileSize + SourceTileSize * 2);

            var w = SourceTileSize * 2;
            var h = SourceTileSize / 4;

            var p = (float)(w * value.GetFactor());

            DrawRect(new Rect2(origin, new Vector2(w, h)), backColor);
            DrawRect(new Rect2(origin, new Vector2(p, h)), foreColor);
        }
    }

    private void DrawFeature(Map map)
        => StampEntity(map.Feature, FeatureAtlas, new(1, 2), (feature) => (int)feature.Type);

    private void DrawItem(Map map)
        => StampEntity(map.Item, ItemAtlas, new(1, 1), (item) => (int)item.Type);

    private void DrawAgent(Map map)
        => StampEntity(map.Agent, AgentAtlas, new(1, 2), (agent) => (int)agent.Definition.Type);

    private void StampEntity<T>(IReadOnlySparseGrid<T> entity, Texture2D atlas, Vector2I stampSize, Func<T, int> stampIdSelector) where T : class
    {
        foreach (var (coordinate, value) in entity)
        {
            int srcX = 0;
            int srcY = stampIdSelector(value) * SourceTileSize * stampSize.Y;

            var dstX = coordinate.X * SourceTileSize;
            var dstY = coordinate.Y * SourceTileSize - (stampSize.Y - 1);

            var size = stampSize * SourceTileSize;

            DrawTextureRectRegion(
                atlas,
                new Rect2I(
                    dstX,
                    dstY,
                    size),
                new Rect2I(
                    srcX,
                    srcY,
                    size));
        }
    }

    private static Vector2 ToVector2(Coordinate coordinate) =>
        new(coordinate.X, coordinate.Y);

    private static Vector2I ToVector2I(Coordinate coordinate) =>
        new(coordinate.X, coordinate.Y);

    public override void _Draw()
    {
        if (_map is null)
        {
            return;
        }

        DrawCache(_map);
        DrawFeature(_map);
        DrawItem(_map);
        DrawAgent(_map);
        DrawItemAggregate(_map);
        DrawWork(_map);
    }
}
