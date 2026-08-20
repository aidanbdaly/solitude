using Godot;

public partial class ToolbarComponent : PanelContainer
{
    private ToolbarTimeComponent _time = null!;
    private ToolbarActionsComponent _actions = null!;
    private ToolbarPlaybackComponent _playback = null!;

    [Signal] public delegate void SpeedRequestedEventHandler(float speed);
    [Signal] public delegate void ToolRequestedEventHandler(int mode);

    public override void _Ready()
    {
        _time = GetNode<ToolbarTimeComponent>("%Time");
        _actions = GetNode<ToolbarActionsComponent>("%Actions");
        _playback = GetNode<ToolbarPlaybackComponent>("%Playback");

        _actions.ToolRequested += mode => EmitSignal(SignalName.ToolRequested, mode);
        _playback.SpeedRequested += speed => EmitSignal(SignalName.SpeedRequested, speed);
    }

    public void SetTime(int day, int hour, int minute) => _time.SetTime(day, hour, minute);

    public void SetTimeStep(float timeStep) => _playback.SetTimeStep(timeStep);

    public void SetMode(CursorMode mode) => _actions.SetMode(mode);
}
