
using Godot;

public class AudioEvent { }

public class AudioEventRouter
{
    [Export] public AudioStream? FloraDamageSound { get; set; }
    [Export] public AudioStream? RockDamageSound { get; set; }
    [Export] public AudioStream? FloraDestroyedSound { get; set; }
    [Export] public AudioStream? RockDestroyedSound { get; set; }
    [Export] public AudioStream? ConstructionCompletedSound { get; set; }

    private readonly AudioStreamPlayer _audioStreamPlayer;

    public AudioEventRouter(AudioStreamPlayer audioStreamPlayer)
    {
        _audioStreamPlayer = audioStreamPlayer;
    }

    public void Route(AudioEvent audio)
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
        _audioStreamPlayer.Stream = sound;
        _audioStreamPlayer.Play();
    }
}