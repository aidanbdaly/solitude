using Godot;

public class InputEventRouter
{
    private static readonly StringName PauseAction = "pause_simulation";
    private static readonly StringName SlowerAction = "simulation_slower";
    private static readonly StringName FasterAction = "simulation_faster";
    private static readonly StringName MoveLeftAction = "camera_left";
    private static readonly StringName MoveRightAction = "camera_right";
    private static readonly StringName MoveUpAction = "camera_up";
    private static readonly StringName MoveDownAction = "camera_down";
    private static readonly StringName ZoomInAction = "camera_zoom_in";
    private static readonly StringName ZoomOutAction = "camera_zoom_out";

    private readonly PlayerService _player;

    public InputEventRouter(PlayerService player)
    {
        _player = player;
    }

    public void Route(InputEvent inputEvent)
    {
        var direction = Godot.Input.GetVector(MoveLeftAction, MoveRightAction, MoveUpAction, MoveDownAction);

        if (direction != Vector2.Zero)
        {
            _player.Pan(direction);
        }

        if (inputEvent.IsActionPressed(ZoomInAction))
        {
            _player.ZoomIn();
        }

        else if (inputEvent.IsActionPressed(ZoomOutAction))
        {
            _player.ZoomOut();
        }

        else if (inputEvent is InputEventMouseMotion motion && motion.ButtonMask.HasFlag(MouseButtonMask.Middle))
        {
            _player.PanRelatve(motion.Relative);
        }

        if (inputEvent is InputEventKey { Echo: true }) return;

        if (inputEvent.IsActionPressed(PauseAction)) _player.TogglePause();
        else if (inputEvent.IsActionPressed(FasterAction)) _player.SetTimeStep(prev => prev + 1);
        else if (inputEvent.IsActionPressed(SlowerAction)) _player.SetTimeStep(prev => prev - 1);
    }

}