using System.Collections.Generic;
 

public readonly record struct Coordinate(int X, int Y);

public sealed record ItemCount(ItemType Type, int Count);

public sealed record AgentPlacement(Coordinate Coordinate, long AgentId);

public sealed record ItemPlacement(Coordinate Coordinate, long ItemId);

public sealed record FeaturePlacement(Coordinate Coordinate, FeatureType Type);

public sealed record WorkPlacement(Coordinate Coordinate, int Required, int Current);

public sealed record ItemAggregatePlacement(
    Coordinate Coordinate,
    IReadOnlyList<ItemRequirement> Required,
    IReadOnlyList<ItemCount> Contents);
