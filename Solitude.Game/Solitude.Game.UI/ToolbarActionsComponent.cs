using System.Collections.Generic;
using Godot;

public enum CursorMode
{
    Command,
    Construct,
    Damage
}

public partial class ToolbarActionsComponent : HBoxContainer
{
    private readonly Dictionary<CursorMode, Button> _buttons = new();

    [Signal] public delegate void ToolRequestedEventHandler(int mode);

    public override void _Ready()
    {
        _buttons[CursorMode.Command] = GetNode<Button>("%Command");
        _buttons[CursorMode.Construct] = GetNode<Button>("%Construct");
        _buttons[CursorMode.Damage] = GetNode<Button>("%Damage");

        foreach (var (mode, button) in _buttons)
        {
            button.Pressed += () => EmitSignal(SignalName.ToolRequested, (int)mode);
        }
    }

    public void SetMode(CursorMode mode)
    {
        foreach (var (buttonMode, button) in _buttons)
        {
            button.ButtonPressed = buttonMode == mode;
        }
    }
}
