using Godot;

public class Agent
{
    public required AgentId Id { get; init; }
    public required string Name { get; init; }
    public required Vector2 Position { get; init; }
    public required AgentDefinition Definition { get; init; }
    public required AgentStatus Status { get; init; } // Formally needs
    public required AgentNavigation Navigation { get; init; } = new();
    public required Inventory Inventory { get; init; } = new();
    public AgentPlan? Plan { get; init; }
}
