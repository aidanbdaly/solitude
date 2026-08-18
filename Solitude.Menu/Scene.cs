using Godot;

public partial class MenuScreen : Control
{
    [Signal] public delegate void BeginRequestedEventHandler();

    private AudioStreamPlayer _music = null!;
    private AudioStreamPlayer _buttonSound = null!;

    public override void _Ready()
    {
        _music = GetNode<AudioStreamPlayer>("MenuMusic");
        _buttonSound = GetNode<AudioStreamPlayer>("ButtonSound");
        _music.Finished += () => _music.Play();
        GetNode<Button>("Buttons/Begin").Pressed += () =>
        {
            _buttonSound.Play();
            EmitSignal(SignalName.BeginRequested);
        };
        GetNode<Button>("Buttons/Quit").Pressed += () =>
        {
            _buttonSound.Play();
            GetTree().Quit();
        };
        _music.Play();
    }

    public void StopMusic() => _music.Stop();

    public override void _ExitTree()
    {
        _music.Stop();
        _music.Stream = null;
        _buttonSound.Stop();
        _buttonSound.Stream = null;
    }
}
