using Godot;
using System;

public partial class GameScene : Node
{
    private GameState _state = null!;
    private PlayerService _playerService = null!;
    private SimulationService _simulationService = null!;
    private AudioEventRouter _audio = null!;
    private InputEventRouter _input = null!;
    private HudComponent _hud = null!;

    public override void _Ready()
    {
        if (_state is null) throw new InvalidOperationException("Game must be initialized with a State before entering the scene tree.");

        _simulationService = new SimulationService(_state);

        GetNode<SimulationNode>("SimulationNode").Set("_simulationService", _simulationService);

        _playerService = new PlayerService(new(), GetNode<Camera>("Camera"), _simulationService); 
        
        _input = new InputEventRouter(_playerService);
        _audio = new AudioEventRouter(GetNode<AudioStreamPlayer>("WorldSounds"));

        _hud = GetNode<HudComponent>("UI/Hud");
        _hud.Toolbar.SpeedRequested += speed => _playerService.SetTimeStep(speed);
    }

    public override void _UnhandledInput(InputEvent inputEvent) => _input.Route(inputEvent);

    private void HandleAudio(AudioEvent audioEvent) => _audio.Route(audioEvent);
}
