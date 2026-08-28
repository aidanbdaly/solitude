using Godot;

public partial class MenuScene : Control
{
    [Signal] public delegate void NewGameRequestedEventHandler(NewGameRequest request);
    [Signal] public delegate void LoadGameRequestedEventHandler(string name);
    [Signal] public delegate void QuitGameRequestedEventHandler();

    public override void _Ready()
    {
        GetNode<Button>("Buttons/New").Pressed += () => EmitSignal(SignalName.NewGameRequested, NewGameRequest.Default);
        GetNode<Button>("Buttons/Load").Pressed += () => EmitSignal(SignalName.LoadGameRequested, "default");
        GetNode<Button>("Buttons/Quit").Pressed += () => EmitSignal(SignalName.QuitGameRequested);
    }
}
