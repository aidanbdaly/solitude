using Godot;

public class Agent
{
    public required long Id { get; init; }
    public required AgentType Type;

    
    public required AgentParameters Definition { get; init; }
    public required AgentStatus Status { get; init; }
    public required AgentNavigation Navigation { get; init; } = new();
    public required Inventory Inventory { get; init; } = new();
}
