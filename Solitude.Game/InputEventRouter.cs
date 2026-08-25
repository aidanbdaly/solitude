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

    private readonly GameService _game;

    public InputEventRouter(GameService game)
    {
        _game = game;
    }

    public void Route(InputEvent inputEvent)
    {
        var direction = Input.GetVector(MoveLeftAction, MoveRightAction, MoveUpAction, MoveDownAction);

        if (direction != Vector2.Zero)
        {
            _game.Pan(direction);
        }

        if (inputEvent.IsActionPressed(ZoomInAction))
        {
            _game.ZoomIn();
        }

        else if (inputEvent.IsActionPressed(ZoomOutAction))
        {
            _game.ZoomOut();
        }

        else if (inputEvent is InputEventMouseMotion motion && motion.ButtonMask.HasFlag(MouseButtonMask.Middle))
        {
            _game.PanRelatve(motion.Relative);
        }

        if (inputEvent is InputEventKey { Echo: true }) return;

        if (inputEvent.IsActionPressed(PauseAction)) _game.TogglePause();
        else if (inputEvent.IsActionPressed(FasterAction)) _game.SetTimeStep(prev => prev + 1);
        else if (inputEvent.IsActionPressed(SlowerAction)) _game.SetTimeStep(prev => prev - 1);
    }

}