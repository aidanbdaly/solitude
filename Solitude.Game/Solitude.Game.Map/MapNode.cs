using System;
using Godot;

public partial class MapNode : Node2D
{
    private const int SourceTileSize = 16;

    [Export] public Texture2D TileAtlas { get; set; } = null!;
    [Export] public Texture2D FeatureAtlas { get; set; } = null!;
    [Export] public Texture2D ItemAtlas { get; set; } = null!;
    [Export] public Texture2D AgentAtlas { get; set; } = null!;

    private Texture2D _cache = null!;
    private MapState _map = null!;

    public override void _Ready()
    {
        var camera = GetNode<Camera>("Camera");

        camera.Set("_surface", _map.GetSize() * SourceTileSize);
        camera.Center();

        TextureFilter = TextureFilterEnum.Nearest;

        // Todo, extract domain behaviour from map into services
        // link up saving state injection
        // look at item aggregate
        // define simple processes
        // test it works
        // then add pathfinding/complex agent behaviour
    }

    public override void _Process(double delta)
    {
        QueueRedraw();
    }

    private void CacheStatic()
    {
        var atlas = TileAtlas.GetImage();
        var cache = Image.CreateEmpty(
            _map.Width * SourceTileSize,
            _map.Height * SourceTileSize,
            false,
            atlas.GetFormat()
        );

        foreach (var (coordinate, tile) in _map.Tile)
        {
            int srcX;
            int srcY = (int)tile.Type * SourceTileSize;

            if (coordinate.Y != 0)
            {
                srcX = (_map.GetTile(new(coordinate.X, coordinate.Y - 1)).Type == tile.Type ? 1 : 0) * SourceTileSize;
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
                coordinate * SourceTileSize);
        }

        _cache = ImageTexture.CreateFromImage(cache);
    }

    private void DrawCache()
        => DrawTextureRect(_cache, new Rect2(Vector2.Zero, _map.GetSize() * SourceTileSize), false);

    private void DrawWork()
    {
        foreach (var (coordinate, value) in _map.Work)
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

    private void DrawFeature()
        => StampEntity(_map.Feature, FeatureAtlas, new(1, 2), (feature) => (int)feature.Type);

    private void DrawItem()
        => StampEntity(_map.Item, ItemAtlas, new(1, 1), (item) => (int)item.Type);

    private void DrawAgent()
        => StampEntity(_map.Agent, AgentAtlas, new(1, 2), (agent) => (int)agent.Type);

    private void StampEntity<T>(IReadOnlyMapEntity<T> entity, Texture2D atlas, Vector2I stampSize, Func<T, int> stampIdSelector) where T : class
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

    public override void _Draw()
    {
        DrawCache();
        DrawFeature();
        DrawItem();
        DrawAgent();
        DrawWork();
    }
}