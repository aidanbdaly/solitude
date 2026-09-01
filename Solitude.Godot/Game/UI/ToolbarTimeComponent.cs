using Godot;

public partial class ToolbarTimeComponent : Label
{
    public void SetTime(int newTime)
    {
        var normalizedDayTime = newTime % Time.CycleLength / Time.CycleLength;

        var day = (int)(newTime / Time.CycleLength) + 1;
        var hour = (int)(normalizedDayTime * 24f);
        var minute = (int)(normalizedDayTime * 24f * 60f) % 60;

        Text = $"Day {day} · {hour:00}:{minute:00}";
    }
}
