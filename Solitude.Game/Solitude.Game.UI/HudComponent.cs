using Godot;

public partial class HudComponent : Control
{
    public CardComponent Card { get; private set; } = null!;
    public ToolbarComponent Toolbar { get; private set; } = null!;

    public override void _Ready()
    {
        Card = GetNode<CardComponent>("%Card");
        Toolbar = GetNode<ToolbarComponent>("%Toolbar");
    }
}
