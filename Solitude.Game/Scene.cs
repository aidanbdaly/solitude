using Godot;
using System;

public partial class GameScene : Node
{
    private GameState _state = null!;
    private GameService _game = null!;
    private AudioEventRouter _audio = null!;
    private InputEventRouter _input = null!;
    private HudComponent _hud = null!;

    public override void _Ready()
    {
        if (_state is null) throw new InvalidOperationException("Game must be initialized with a State before entering the scene tree.");

        GetNode<MapNode>("SimulationNode").Set("_simulationService", _state);
    
        _game = new GameService(_state, GetNode<Camera>("Camera"), GetNode<HudComponent>("UI/Hud"));

        _input = new InputEventRouter(_game);
        _audio = new AudioEventRouter(GetNode<AudioStreamPlayer>("WorldSounds"));
    }

    public override void _UnhandledInput(InputEvent inputEvent) => _input.Route(inputEvent);

    private void HandleAudio(AudioEvent audioEvent) => _audio.Route(audioEvent);
}
