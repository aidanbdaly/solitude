using System;
using Godot;

public class PlayerService
{
    private readonly PlayerState _state;

    private readonly Camera _camera;

    private readonly SimulationService _simulation;

    public PlayerService(PlayerState state, Camera camera, SimulationService simulation)
    {
        _state = state;
        _camera = camera;
        _simulation = simulation;
    }

    public void TogglePause() => _simulation.TogglePause();

    public void SetTimeStep(float timeStep) => _simulation.SetTimeStep(timeStep);

    public void SetTimeStep(Func<float, float> setStateDelegate) => _simulation.SetTimeStep(setStateDelegate);

    public void Pan(Vector2 direction) => _camera.Pan(direction);

    public void PanRelatve(Vector2 offset) => _camera.PanRelatve(offset);

    public void ZoomIn() => _camera.ZoomIn();

    public void ZoomOut() => _camera.ZoomOut();

}