using Godot;

public sealed partial class GameState : Resource
{
    public required Map Map { get; init; }
    public required Time Time { get; init; }
    public bool Paused { get; set; } = true;
    public float TimeStep { get; set; } = 1f;
}