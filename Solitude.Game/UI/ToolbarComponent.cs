using Godot;

public partial class ToolbarComponent : PanelContainer
{
    private ToolbarTimeComponent _time = null!;
    private ToolbarActionsComponent _actions = null!;
    private ToolbarPlaybackComponent _playback = null!;

    [Signal] public delegate void ToolRequestedEventHandler(int mode);

    [Signal] public delegate void SimulationSpeedRequestedEventHandler(float simulationSpeed);

    public override void _Ready()
    {
        _time = GetNode<ToolbarTimeComponent>("%Time");
        _actions = GetNode<ToolbarActionsComponent>("%Actions");
        _playback = GetNode<ToolbarPlaybackComponent>("%Playback");

        _actions.ToolRequested += mode => EmitSignal(SignalName.ToolRequested, mode);
        _playback.SimulationSpeedRequested += simulationSpeed => EmitSignal(SignalName.SimulationSpeedRequested, simulationSpeed);
        
    }

    public void SetTime(int newTime) => _time.SetTime(newTime);

    public void SetSimulationSpeed(float simulationSpeed) => _playback.SetSimulationSpeed(simulationSpeed);

    public void SetMode(CursorMode mode) => _actions.SetMode(mode);
}
