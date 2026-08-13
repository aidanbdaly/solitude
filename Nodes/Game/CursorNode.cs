using Godot;
using System.Linq;
using Solitude.Domain.Game;
using Solitude.Domain.Game.Commands;
using Solitude.Domain.Game.Agents;
using Solitude.Domain.Game.Map;

namespace Solitude.Nodes.Game;

public partial class CursorNode : Node2D
{
    private static readonly StringName PrimaryAction = "cursor_primary";
    private static readonly StringName SecondaryAction = "cursor_secondary";
    private static readonly StringName CommandAction = "tool_command";
    private static readonly StringName ConstructAction = "tool_construct";
    private static readonly StringName DamageAction = "tool_damage";

    private Grid _grid = null!;
    private World _world = null!;
    private PlayerCommandService _commands = null!;
    private float _tileSize;

    public CursorMode Mode { get; private set; } = CursorMode.Command;
    public Vector2I HoveredCell { get; private set; } = new(-1, -1);
    public AgentId? SelectedAgentId { get; private set; }

    [Signal] public delegate void ModeChangedEventHandler(int mode);
    [Signal] public delegate void SelectionChangedEventHandler();

    public void Initialize(State state, float tileSize, PlayerCommandService commands)
    {
        _grid = state.World.Grid;
        _world = state.World;
        _commands = commands;
        _tileSize = tileSize;
    }

    public void SetMode(CursorMode mode)
    {
        Mode = mode;
        EmitSignal(SignalName.ModeChanged, (int)mode);
    }

    public void PrimaryClick()
    {
        switch (Mode)
        {
            case CursorMode.Command:
                SelectAgent(HoveredCell);
                break;
            case CursorMode.Construct:
                _commands.Construct(HoveredCell);
                break;
            case CursorMode.Damage:
                _commands.Damage(HoveredCell);
                break;
        }
        EmitSignal(SignalName.SelectionChanged);
    }

    public void SecondaryClick()
    {
        if (Mode == CursorMode.Command && SelectedAgentId is { } agentId)
            _commands.Move(agentId, HoveredCell);
    }

    public override void _Process(double delta)
    {
        if (_grid is null) return;
        var worldPosition = GetGlobalMousePosition();
        HoveredCell = new Vector2I(
            Mathf.FloorToInt(worldPosition.X / _tileSize),
            Mathf.FloorToInt(worldPosition.Y / _tileSize));
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_grid is null) return;

        if (_grid.Contains(HoveredCell))
        {
            var color = CanExecutePrimary(HoveredCell)
                ? new Color(1f, 0.86f, 0.28f, 0.75f)
                : new Color(1f, 0.25f, 0.2f, 0.75f);
            DrawRect(new Rect2(HoveredCell.X * _tileSize, HoveredCell.Y * _tileSize, _tileSize, _tileSize),
                color, false, 2f);
        }

        if (SelectedAgentId is not { } agentId || !_world.TryGetAgent(agentId, out var agent)) return;
        var center = (agent.Position + new Vector2(0.5f, 0.72f)) * _tileSize;
        DrawArc(center, 12f, 0f, Mathf.Tau, 32, new Color(1f, 0.83f, 0.25f), 2f, true);
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (_grid is null || inputEvent is InputEventKey { Echo: true }) return;
        if (inputEvent.IsActionPressed(PrimaryAction))
        {
            PrimaryClick();
            return;
        }
        if (inputEvent.IsActionPressed(SecondaryAction))
        {
            SecondaryClick();
            return;
        }

        if (inputEvent.IsActionPressed(CommandAction) || inputEvent.IsActionPressed("ui_cancel"))
            SetMode(CursorMode.Command);
        else if (inputEvent.IsActionPressed(ConstructAction)) SetMode(CursorMode.Construct);
        else if (inputEvent.IsActionPressed(DamageAction)) SetMode(CursorMode.Damage);
    }

    private bool CanExecutePrimary(Vector2I cell) => Mode switch
    {
        CursorMode.Command => _grid.Contains(cell),
        CursorMode.Construct => _commands.CanConstruct(cell),
        CursorMode.Damage => _commands.CanDamage(cell),
        _ => false
    };

    private void SelectAgent(Vector2I cell)
    {
        var agent = _world.AgentsAt(cell)
            .OrderByDescending(candidate => candidate.Id.Value)
            .FirstOrDefault();
        SelectedAgentId = agent?.Id;
    }
}
