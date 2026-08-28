public class AgentStatus
{
    public required float Tiredness { get; init; }
    public required float Hunger { get; init; }

    public static AgentStatus Default => new()
    {
        Tiredness = 0.2f,
        Hunger = 0.2f
    };
}