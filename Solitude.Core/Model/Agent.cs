public class Agent
{
    public required long Id { get; init; }
    
    public required AgentDefinition Definition { get; init; }
    public required AgentStatus Status { get; init; }
    public required AgentInventory Inventory { get; init; }
}
