
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

        _audioStreamPlayer.Stream = sound;
        _audioStreamPlayer.Play();
    }
}