using Godot;
using Solitude.Domain.Game.Map;

namespace Solitude.Nodes.Game;

public partial class CameraNode : Camera2D
{
	private static readonly StringName MoveLeftAction = "camera_left";
	private static readonly StringName MoveRightAction = "camera_right";
	private static readonly StringName MoveUpAction = "camera_up";
	private static readonly StringName MoveDownAction = "camera_down";
	private static readonly StringName ZoomInAction = "camera_zoom_in";
	private static readonly StringName ZoomOutAction = "camera_zoom_out";

	[Export] public float PanSpeed { get; set; } = 650f;
	[Export] public float InitialZoom { get; set; } = 1.2f;
	[Export] public float MinimumZoom { get; set; } = 0.35f;
	[Export] public float MaximumZoom { get; set; } = 4f;
	[Export] public float ZoomStep { get; set; } = 1.12f;

	private Grid _grid = null!;
	private float _tileSize;

	public void Initialize(Grid grid, float tileSize)
	{
		_grid = grid;
		_tileSize = tileSize;
		Position = new Vector2(grid.Width, grid.Height) * tileSize * 0.5f;
		Zoom = Vector2.One * InitialZoom;
		Enabled = true;
		ResetSmoothing();
	}

	public override void _Process(double delta)
	{
		if (_grid is null) return;
		var direction = Godot.Input.GetVector(MoveLeftAction, MoveRightAction, MoveUpAction, MoveDownAction);
		Position += direction * (PanSpeed / Zoom.X) * (float)delta;
		Position = new Vector2(
			Mathf.Clamp(Position.X, 0f, _grid.Width * _tileSize),
			Mathf.Clamp(Position.Y, 0f, _grid.Height * _tileSize));
	}

	public override void _UnhandledInput(InputEvent inputEvent)
	{
		if (_grid is null) return;
		if (inputEvent.IsActionPressed(ZoomInAction)) SetZoom(Zoom.X * ZoomStep);
		else if (inputEvent.IsActionPressed(ZoomOutAction)) SetZoom(Zoom.X / ZoomStep);
		else if (inputEvent is InputEventMouseMotion motion && motion.ButtonMask.HasFlag(MouseButtonMask.Middle))
			Position -= motion.Relative / Zoom.X;
	}

	private void SetZoom(float zoom)
	{
		zoom = Mathf.Clamp(zoom, MinimumZoom, MaximumZoom);
		Zoom = Vector2.One * zoom;
	}
}
