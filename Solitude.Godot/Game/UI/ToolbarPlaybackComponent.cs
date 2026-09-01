using System.Collections.Generic;
using Godot;

public partial class ToolbarPlaybackComponent : HBoxContainer
{
    private readonly Dictionary<float, Button> _speedButtons = new();

    [Signal] public delegate void SimulationSpeedRequestedEventHandler(float simulationSpeed);

    public override void _Ready()
    {
        _speedButtons[0f] = GetNode<Button>("%Pause");
        _speedButtons[0.5f] = GetNode<Button>("%HalfSpeed");
        _speedButtons[1f] = GetNode<Button>("%NormalSpeed");
        _speedButtons[2f] = GetNode<Button>("%DoubleSpeed");
        _speedButtons[4f] = GetNode<Button>("%QuadrupleSpeed");

        foreach (var (simulationSpeed, button) in _speedButtons)
        {
            button.Pressed += () => EmitSignal(SignalName.SimulationSpeedRequested, simulationSpeed);
        }
    }

    public void SetSimulationSpeed(float simulationSpeed)
    {
        foreach (var (speed, button) in _speedButtons)
        {
            button.ButtonPressed = Mathf.IsEqualApprox(speed, simulationSpeed);
        }
    }
}
