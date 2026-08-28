using System.Collections.Generic;

public sealed record AgentSnapshot(
    long Id,
    AgentDefinition Definition,
    AgentStatus Status,
    IReadOnlyList<ItemCount> Inventory);