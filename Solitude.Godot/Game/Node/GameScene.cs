using System;
using Godot;

public partial class GameScene : Node
{
    private MapNode _mapNode = null!;
    private ToolbarComponent _toolbar = null!;

    private Game _game = null!;

    public override void _Ready()
    {
        _mapNode = GetNode<MapNode>("Map");
        _mapNode.TimeChanged += OnTimeChanged;

        _toolbar = GetNode<ToolbarComponent>("UI/Toolbar");
        _toolbar.SimulationSpeedRequested += OnSimulationSpeedRequested;
    }

    public override void _ExitTree()
    {
        if (_game is not null)
        {
            _game.MapChanged -= OnMapChanged;
        }

        _mapNode.TimeChanged -= OnTimeChanged;
        _toolbar.SimulationSpeedRequested -= OnSimulationSpeedRequested;
    }

    public void Bind(Game game)
    {
        if (!IsNodeReady())
        {
            throw new InvalidOperationException(
                "GameScene must be added to the scene tree before binding.");
        }

        if (_game is not null)
        {
            _game.MapChanged -= OnMapChanged;
        }

        _game = game;
        _game.MapChanged += OnMapChanged;

        _mapNode.Bind(game.GetActiveMap());
    }

    private void OnMapChanged(object? e, MapChangedEvent evt) => _mapNode.Bind(evt.NewMap);

    private void OnTimeChanged(int newTime) => _toolbar.SetTime(newTime);

    private void OnSimulationSpeedRequested(float simulationSpeed)
    {
        _mapNode.SetSimulationSpeed(simulationSpeed);
        _toolbar.SetSimulationSpeed(simulationSpeed);
    }
}
