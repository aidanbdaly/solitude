
using Godot;
public partial class Camera : Camera2D
{
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
        ResetSmoothing();
    }

    public void Center()
    {
        Position = _surface * 0.5f;
    }

    public void Pan(Vector2 direction)
    {
        Position += direction * (PanSpeed / Zoom.X);
        Position = new Vector2(
            Mathf.Clamp(Position.X, 0f, _surface.X),
            Mathf.Clamp(Position.Y, 0f, _surface.Y));
    }

    public void PanRelatve(Vector2 offset)
    {
        Position -= offset / Zoom.X;
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
}
