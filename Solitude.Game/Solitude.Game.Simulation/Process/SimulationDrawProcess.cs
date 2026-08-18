using Godot;

public partial class SimulationDrawProcess : Node2D
{
    private const int SourceTileSize = 16;

    [Export] public Texture2D TileAtlas { get; set; } = null!;
    [Export] public Texture2D RockTexture { get; set; } = null!;
    [Export] public Texture2D FloraTexture { get; set; } = null!;
    [Export] public Texture2D WallTexture { get; set; } = null!;
    [Export] public Texture2D TaskTexture { get; set; } = null!;
    [Export] public Texture2D ItemTexture { get; set; } = null!;
    [Export] public Texture2D AgentTexture { get; set; } = null!;

}
