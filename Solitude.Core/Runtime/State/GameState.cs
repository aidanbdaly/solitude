using System;
using Godot;

public sealed partial class GameState : Resource
{
    private int _nextAgentId = -1;
    private int _nextItemId = -1;

    public required PlayerState Player { get; init; }
    public required MapState Map { get; init; }
    public required Time Time { get; init; }

    public bool Paused { get; set; } = true;
    public float TimeStep { get; set; } = 1f;

    public int NextAgentId => _nextAgentId++;
    public int NextItemId => _nextItemId++;

    public void TogglePause() { Paused = !Paused; }

    public void SetTimeStep(float timeStep) { TimeStep = timeStep; }

    public void SetTimeStep(Func<float, float> setStateDelegate) { TimeStep = setStateDelegate(TimeStep); }


}