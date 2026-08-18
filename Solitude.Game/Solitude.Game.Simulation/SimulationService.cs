using System;
using Godot;

public static class SimulationConstant
{
    public const float TileResolutionPX = 32f;
}

public partial class SimulationService(SimulationState State) : GodotObject
{
    private readonly SimulationState _state = State;

    public void TogglePause() { _state.Paused = !_state.Paused; }

    public void SetTimeStep(float timeStep) { _state.TimeStep = timeStep; }

    public void SetTimeStep(Func<float, float> setStateDelegate) { _state.TimeStep = setStateDelegate(_state.TimeStep); }

    public Vector2 GetMapSize()
    {
        return new Vector2(
            _state.Map.Width,
            _state.Map.Height
        );
    }
}