using System;
using Godot;

public partial class MenuScene : Control
{
    public event Action<NewGameRequest>? NewGameRequested;
    public event Action<string>? LoadGameRequested;
    public event Action? QuitGameRequested;

    public override void _Ready()
    {
        GetNode<Button>("Buttons/New").Pressed += () => NewGameRequested?.Invoke(NewGameRequest.Default);
        GetNode<Button>("Buttons/Load").Pressed += () => LoadGameRequested?.Invoke(UserData.DefaultSlot);
        GetNode<Button>("Buttons/Quit").Pressed += () => QuitGameRequested?.Invoke();
    }
}
