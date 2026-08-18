using Godot;
using System;



public partial class Scene : Node
{
    private SaveState _state = null!;
    private PlayerService _playerService = null!;
    private SimulationService _simulationService = null!;
    private AudioEventRouter _audio = null!;
    private InputEventRouter _input = null!;

    public override void _Ready()
    {
        if (_state is null) throw new InvalidOperationException("Game must be initialized with a State before entering the scene tree.");

        _simulationService = new SimulationService(_state.simulation);

        GetNode<SimulationNode>("SimulationNode").Set("_simulationService", _simulationService);

        _playerService = new PlayerService(new(), GetNode<Camera>("Camera"), _simulationService); // Authoritative

        _input = new InputEventRouter(_player);
        _audio = new AudioEventRouter(GetNode<AudioStreamPlayer>("WorldSounds"));

        GetNode<HudNode>("UI/Hud").Set("_playerService", _playerService);
    }

    public override void _UnhandledInput(InputEvent inputEvent) => _input.Route(inputEvent);

    private void HandleAudio(AudioEvent audioEvent) => _audio.Route(audioEvent);
}
