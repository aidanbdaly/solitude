using System.Collections.Generic;
using Godot;

public partial class ToolbarPlaybackComponent : HBoxContainer
{
    private readonly Dictionary<float, Button> _speedButtons = new();
    private Button _pauseButton = null!;

    [Signal] public delegate void SpeedRequestedEventHandler(float speed);

    public override void _Ready()
    {
        _pauseButton = GetNode<Button>("%Pause");
        _speedButtons[0.5f] = GetNode<Button>("%HalfSpeed");
        _speedButtons[1f] = GetNode<Button>("%NormalSpeed");
        _speedButtons[2f] = GetNode<Button>("%DoubleSpeed");
        _speedButtons[4f] = GetNode<Button>("%QuadrupleSpeed");

        _pauseButton.Pressed += () => EmitSignal(SignalName.SpeedRequested, 0f);
        foreach (var (speed, button) in _speedButtons)
        {
            button.Pressed += () => EmitSignal(SignalName.SpeedRequested, speed);
        }
    }

    public void SetTimeStep(float timeStep)
    {
        _pauseButton.ButtonPressed = timeStep <= 0f;

        foreach (var (speed, button) in _speedButtons)
        {
            button.ButtonPressed = timeStep > 0f && Mathf.IsEqualApprox(speed, timeStep);
        }
    }
}
