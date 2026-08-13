using Godot;
using System.Collections.Generic;
using Solitude.Domain.Game;
using Solitude.Domain.Game.Items;

namespace Solitude.Nodes.Game;

public partial class HudNode : Control
{
    private readonly Dictionary<CursorMode, Button> _toolButtons = new();
    private readonly Dictionary<float, Button> _speedButtons = new();

    private World _world = null!;
    private Clock _clock = null!;
    private CursorNode _cursor = null!;
    private Control _selectionPanel = null!;
    private Label _selectionName = null!;
    private Label _selectionInventory = null!;
    private Label _timeLabel = null!;
    private Button _pauseButton = null!;

    [Signal] public delegate void PauseRequestedEventHandler();
    [Signal] public delegate void SpeedRequestedEventHandler(float speed);

    public override void _Ready()
    {
        _selectionPanel = GetNode<Control>("SelectionPanel");
        _selectionName = GetNode<Label>("SelectionPanel/Content/Name");
        _selectionInventory = GetNode<Label>("SelectionPanel/Content/Inventory");
        _timeLabel = GetNode<Label>("BottomToolbar/Margin/Row/Time");

        const string toolPath = "BottomToolbar/Margin/Row/Tools";
        _toolButtons[CursorMode.Command] = GetNode<Button>($"{toolPath}/Command");
        _toolButtons[CursorMode.Construct] = GetNode<Button>($"{toolPath}/Construct");
        _toolButtons[CursorMode.Damage] = GetNode<Button>($"{toolPath}/Damage");

        const string playbackPath = "BottomToolbar/Margin/Row/Playback";
        _pauseButton = GetNode<Button>($"{playbackPath}/Pause");
        _speedButtons[0.5f] = GetNode<Button>($"{playbackPath}/HalfSpeed");
        _speedButtons[1f] = GetNode<Button>($"{playbackPath}/NormalSpeed");
        _speedButtons[2f] = GetNode<Button>($"{playbackPath}/DoubleSpeed");
        _speedButtons[4f] = GetNode<Button>($"{playbackPath}/QuadrupleSpeed");
    }

    public void Initialize(State state, CursorNode cursor)
    {
        _world = state.World;
        _clock = state.Clock;
        _cursor = cursor;

        foreach (var pair in _toolButtons)
            pair.Value.Pressed += () => cursor.SetMode(pair.Key);
        foreach (var pair in _speedButtons)
            pair.Value.Pressed += () => EmitSignal(SignalName.SpeedRequested, pair.Key);
        _pauseButton.Pressed += () => EmitSignal(SignalName.PauseRequested);

        cursor.ModeChanged += mode => ShowMode((CursorMode)mode);
        cursor.SelectionChanged += RefreshSelection;
        ShowMode(cursor.Mode);
        RefreshSelection();
    }

    public void Refresh(float speed, bool paused)
    {
        if (_world is null) return;
        _timeLabel.Text = $"Day {_clock.Day} · {_clock.Hour:00}:{_clock.Minute:00}";
        _pauseButton.ButtonPressed = paused;
        if (!paused && _speedButtons.TryGetValue(speed, out var speedButton)) speedButton.ButtonPressed = true;
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        var agent = _cursor?.SelectedAgentId is { } id && _world.TryGetAgent(id, out var selected)
            ? selected
            : null;
        _selectionPanel.Visible = agent is not null;
        if (agent is null) return;

        _selectionName.Text = agent.Name;

        var inventory = new List<string>();
        AddInventory(inventory, "Wood", agent.Inventory.GetCount(ItemType.Wood));
        AddInventory(inventory, "Stone", agent.Inventory.GetCount(ItemType.Stone));
        _selectionInventory.Text = string.Join(" · ", inventory);
        _selectionInventory.Visible = inventory.Count > 0;
    }

    private void ShowMode(CursorMode mode)
    {
        foreach (var pair in _toolButtons) pair.Value.ButtonPressed = pair.Key == mode;
    }

    private static void AddInventory(ICollection<string> inventory, string name, int count)
    {
        if (count > 0) inventory.Add($"{name} {count}");
    }
}
