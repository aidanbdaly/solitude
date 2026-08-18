public sealed class SimulationState
{
    public required Map Map { get; init; }
    public required Time Time { get; init; }
    public bool Paused { get; set; } = false;
    public float TimeStep { get; set; } = 1f;
}