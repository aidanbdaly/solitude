using System;
using Godot;

public class GameService
{
    private readonly GameState _game;

    private readonly Camera _camera;

    public GameService(GameState game, Camera camera)
    {
        _game = game;
        _camera = camera;

        _hud.Toolbar.SpeedRequested += speed => _game.SetTimeStep(speed);
    }

    public void TogglePause() => _game.TogglePause();

    public void SetTimeStep(float timeStep) => _game.SetTimeStep(timeStep);

    public void SetTimeStep(Func<float, float> setStateDelegate) => _game.SetTimeStep(setStateDelegate);

    public void Pan(Vector2 direction) => _camera.Pan(direction);

    public void PanRelatve(Vector2 offset) => _camera.PanRelatve(offset);

    public void ZoomIn() => _camera.ZoomIn();

    public void ZoomOut() => _camera.ZoomOut();

}