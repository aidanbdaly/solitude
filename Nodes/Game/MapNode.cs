using Godot;
using System.Linq;
using Solitude.Domain.Game;
using Solitude.Domain.Game.Construction;
using Solitude.Domain.Game.Items;
using Solitude.Domain.Game.Objects;
using Solitude.Domain.Game.Map;
using Solitude.Domain.Game.Orders;

namespace Solitude.Nodes.Game;

public partial class MapNode : Node2D
{
    private const int SourceTileSize = 16;

    [Export] public Texture2D TileAtlas { get; set; } = null!;
    [Export] public Texture2D RockTexture { get; set; } = null!;
    [Export] public Texture2D FloraTexture { get; set; } = null!;
    [Export] public Texture2D WallTexture { get; set; } = null!;
    [Export] public Texture2D TaskTexture { get; set; } = null!;
    [Export] public Texture2D ItemTexture { get; set; } = null!;
    [Export] public Texture2D AgentTexture { get; set; } = null!;

    private Grid _grid = null!;
    private World _world = null!;
    private Clock _clock = null!;
    private CanvasModulate _dayTint = null!;
    private Texture2D _terrainTexture = null!;
    private float _tileSize;

    public override void _Ready() => _dayTint = GetNode<CanvasModulate>("DayNightTint");

    public void Initialize(State state, float tileSize)
    {
        _grid = state.World.Grid;
        _world = state.World;
        _clock = state.Clock;
        _tileSize = tileSize;
        TextureFilter = TextureFilterEnum.Nearest;
        _terrainTexture = BuildTerrainTexture();
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        if (_clock is null) return;
        var light = _clock.LightLevel;
        _dayTint.Color = new Color(0.26f + 0.74f * light, 0.34f + 0.66f * light, 0.52f + 0.48f * light);
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_world is null) return;
        DrawTerrain();
        DrawObjects();
        DrawConstructionSites();
        DrawOrders();
        DrawItems();
        DrawAgents();
    }

    private Texture2D BuildTerrainTexture()
    {
        var atlasImage = TileAtlas.GetImage();
        var terrainImage = Image.CreateEmpty(
            _grid.Width * SourceTileSize,
            _grid.Height * SourceTileSize,
            false,
            atlasImage.GetFormat());

        foreach (var pair in _grid.Cells)
        {
            var source = new Rect2I(
                pair.Value.Variation == TileVariation.Isolated ? SourceTileSize : 0,
                (int)pair.Value.TileType * SourceTileSize,
                SourceTileSize,
                SourceTileSize);
            terrainImage.BlitRect(atlasImage, source, pair.Key * SourceTileSize);
        }

        return ImageTexture.CreateFromImage(terrainImage);
    }

    private void DrawTerrain()
    {
        var destination = new Rect2(Vector2.Zero, new Vector2(_grid.Width, _grid.Height) * _tileSize);
        DrawTextureRect(_terrainTexture, destination, false);
    }

    private void DrawObjects()
    {
        foreach (var obj in _world.Objects
                     .OrderBy(obj => obj.Cell.Y)
                     .ThenBy(obj => obj.Cell.X))
        {
            var texture = obj.Type switch
            {
                MapObjectType.Rock => RockTexture,
                MapObjectType.Flora => FloraTexture,
                _ => WallTexture
            };
            DrawBottomAnchored(texture, obj.Cell, Colors.White);
        }
    }

    private void DrawConstructionSites()
    {
        foreach (var site in _world.ConstructionSites.OrderBy(site => site.Cell.Y))
        {
            var texture = site.BuildingType switch
            {
                BuildingType.Wall => WallTexture,
                _ => WallTexture
            };
            DrawBottomAnchored(texture, site.Cell, new Color(0.75f, 0.85f, 1f, 0.4f));
            DrawConstructionProgress(site);
        }
    }

    private void DrawOrders()
    {
        foreach (var order in _world.Orders)
        {
            if (!TryGetOrderCell(order, out var cell)) continue;
            var atlasIndex = order is ConstructOrder ? 0 : 1;
            DrawTextureRectRegion(
                TaskTexture,
                new Rect2(cell.X * _tileSize, cell.Y * _tileSize, _tileSize, _tileSize),
                new Rect2(atlasIndex * 32f, 0f, 32f, 32f));
        }
    }

    private bool TryGetOrderCell(Order order, out Vector2I cell)
    {
        switch (order)
        {
            case DamageOrder damage when _world.TryGetObject(damage.Target, out var target):
                cell = target.Cell;
                return true;
            case ConstructOrder construct when
                _world.TryGetConstructionSite(construct.Target, out var site):
                cell = site.Cell;
                return true;
            default:
                cell = default;
                return false;
        }
    }

    private void DrawItems()
    {
        foreach (var item in _world.Items.OrderBy(item => item.Cell.Y))
        {
            var color = item.Type == ItemType.Wood ? Colors.White : new Color(0.72f, 0.76f, 0.82f);
            DrawBottomAnchored(ItemTexture, item.Cell, color);
        }
    }

    private void DrawAgents()
    {
        foreach (var agent in _world.Agents.OrderBy(agent => agent.Position.Y))
            DrawBottomAnchored(AgentTexture, agent.Position, Colors.White);
    }

    private void DrawConstructionProgress(ConstructionSite site)
    {
        var progress = site.State.SupplyProgress * 0.55f + site.State.ConstructionProgress * 0.45f;
        var origin = new Vector2(site.Cell.X * _tileSize + 2f, site.Cell.Y * _tileSize + 27f);
        DrawRect(new Rect2(origin, new Vector2(28f, 3f)), new Color(0.08f, 0.1f, 0.12f, 0.85f));
        DrawRect(new Rect2(origin, new Vector2(28f * progress, 3f)), new Color(0.92f, 0.72f, 0.3f));
    }

    private void DrawBottomAnchored(Texture2D texture, Vector2 position, Color color)
    {
        var size = texture.GetSize() * 2f;
        var bottomCenter = (position + new Vector2(0.5f, 1f)) * _tileSize;
        DrawTextureRect(texture, new Rect2(bottomCenter - new Vector2(size.X / 2f, size.Y), size), false, color);
    }
}
