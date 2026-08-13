using Godot;
using System;
using Solitude.Domain.Game;
using Solitude.Domain.Game.Objects;

namespace Solitude.Nodes.Game;

public partial class GameNode : Node
{
    private const float TileSize = 32f;
    private static readonly StringName PauseAction = "pause_simulation";
    private static readonly StringName SlowerAction = "simulation_slower";
    private static readonly StringName FasterAction = "simulation_faster";

    [Export] public AudioStream? FloraDamageSound { get; set; }
    [Export] public AudioStream? RockDamageSound { get; set; }
    [Export] public AudioStream? FloraDestroyedSound { get; set; }
    [Export] public AudioStream? RockDestroyedSound { get; set; }
    [Export] public AudioStream? ConstructionCompletedSound { get; set; }

    private readonly float[] _speeds = { 0.5f, 1f, 2f, 4f };
    private State _state = null!;
    private Simulation _simulation = null!;
    private HudNode _hud = null!;
    private AudioStreamPlayer _soundPlayer = null!;
    private bool _paused;
    private float _speed = 1f;

    public void Initialize(State state) => _state = state;

    public override void _Ready()
    {
        if (_state is null) throw new InvalidOperationException("Game must be initialized with a State before entering the scene tree.");

        _soundPlayer = GetNode<AudioStreamPlayer>("WorldSounds");
        _simulation = new Simulation(_state);
        _simulation.EventOccurred += PlayEvent;

        var map = GetNode<MapNode>("Map");
        var cursor = GetNode<CursorNode>("Cursor");
        var camera = GetNode<CameraNode>("Map/Camera");
        _hud = GetNode<HudNode>("UI/Hud");
        cursor.Initialize(_state, TileSize, _simulation.Commands);
        map.Initialize(_state, TileSize);
        camera.Initialize(_state.World.Grid, TileSize);
        _hud.Initialize(_state, cursor);
        _hud.PauseRequested += TogglePause;
        _hud.SpeedRequested += SetSpeed;
    }

    public override void _Process(double delta)
    {
        if (!_paused) _simulation.Update((float)delta * _speed);
        _hud.Refresh(_speed, _paused);
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (inputEvent is InputEventKey { Echo: true }) return;
        if (inputEvent.IsActionPressed(PauseAction)) TogglePause();
        else if (inputEvent.IsActionPressed(FasterAction)) ChangeSpeed(1);
        else if (inputEvent.IsActionPressed(SlowerAction)) ChangeSpeed(-1);
    }

    private void TogglePause() => _paused = !_paused;

    private void SetSpeed(float speed)
    {
        _speed = speed;
        _paused = false;
    }

    private void ChangeSpeed(int direction)
    {
        var index = Array.IndexOf(_speeds, _speed);
        _speed = _speeds[Math.Clamp(index + direction, 0, _speeds.Length - 1)];
    }

    private void PlayEvent(SimulationEvent simulationEvent)
    {
        var sound = simulationEvent.Type switch
        {
            SimulationEventType.ObjectDamaged when simulationEvent.ObjectType == MapObjectType.Flora => FloraDamageSound,
            SimulationEventType.ObjectDamaged => RockDamageSound,
            SimulationEventType.ObjectDestroyed when simulationEvent.ObjectType == MapObjectType.Flora => FloraDestroyedSound,
            SimulationEventType.ObjectDestroyed => RockDestroyedSound,
            SimulationEventType.ConstructionCompleted => ConstructionCompletedSound,
            _ => null
        };
        if (sound is null) return;
        _soundPlayer.Stream = sound;
        _soundPlayer.Play();
    }
}
