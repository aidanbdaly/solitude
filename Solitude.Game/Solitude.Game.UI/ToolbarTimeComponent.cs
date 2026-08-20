using Godot;

public partial class ToolbarTimeComponent : Label
{
    public void SetTime(int day, int hour, int minute)
    {
        Text = $"Day {day} · {hour:00}:{minute:00}";
    }
}
