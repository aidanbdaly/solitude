
using Godot;

public partial class Camera : Camera2D
{
    private static readonly StringName Left = "camera_left";
    private static readonly StringName Right = "camera_right";
    private static readonly StringName Up = "camera_up";
    private static readonly StringName Down = "camera_down";
    private static readonly StringName ZoomInAction = "camera_zoom_in";
    private static readonly StringName ZoomOutAction = "camera_zoom_out";

    [Export] public float PanSpeed { get; set; } = 650f;
    [Export] public float InitialZoom { get; set; } = 1.2f;
    [Export] public float MinimumZoom { get; set; } = 0.35f;
    [Export] public float MaximumZoom { get; set; } = 4f;
    [Export] public float ZoomStep { get; set; } = 1.12f;

    private Vector2 _surface;

    public override void _Ready()
    {
        Zoom = Vector2.One * InitialZoom;
        Enabled = true;
        
        PositionSmoothingEnabled = true;
        PositionSmoothingSpeed = 8f;
        ResetSmoothing();
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed(ZoomInAction))
        {
            ZoomIn();
        }
        else if (inputEvent.IsActionPressed(ZoomOutAction))
        {
            ZoomOut();
        }
        else if (inputEvent is InputEventMouseMotion motion && motion.ButtonMask.HasFlag(MouseButtonMask.Middle))
        {
            PanRelative(motion.Relative);
        }
    }

    public void SetSurface(Vector2 surface)
    {
        _surface = surface;
        Center();
    }

    public void Center()
    {
        Position = _surface * 0.5f;
    }

    private void Pan(Vector2 direction, float delta)
    {
        Position += direction * (PanSpeed * delta / Zoom.X);
        ClampPosition();
    }

    private void PanRelative(Vector2 offset)
    {
        Position -= offset / Zoom.X;
        ClampPosition();
    }

    private void ClampPosition()
    {
        Position = new Vector2(
            Mathf.Clamp(Position.X, 0f, _surface.X),
            Mathf.Clamp(Position.Y, 0f, _surface.Y));
    }

    public void ZoomIn()
    {
        SetZoom(Zoom.X * ZoomStep);
    }

    public void ZoomOut()
    {
        SetZoom(Zoom.X / ZoomStep);
    }

    private void SetZoom(float zoom)
    {
        zoom = Mathf.Clamp(zoom, MinimumZoom, MaximumZoom);
        Zoom = Vector2.One * zoom;
    }

    public override void _Process(double delta)
    {
        var direction = Input.GetVector(Left, Right, Up, Down);

        if (direction != Vector2.Zero)
        {
            Pan(direction, (float)delta);
        }
    }
}
