using System.Collections.Generic;

public sealed record WorldSnapshot(
    uint Width,
    uint Height,
    long NextAgentId,
    long NextItemId,
    IReadOnlyList<AgentSnapshot> Agents,
    IReadOnlyList<ItemSnapshot> Items,
    IReadOnlyList<MapSnapshot> Maps);
