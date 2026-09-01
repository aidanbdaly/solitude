using System.Collections.Generic;
 
public sealed record MapSnapshot(
    Coordinate WorldCoordinate,
    int Width,
    int Height,
    uint Time,
    IReadOnlyList<TileType> Tiles,
    IReadOnlyList<FeaturePlacement> Features,
    IReadOnlyList<WorkPlacement> Work,
    IReadOnlyList<ItemPlacement> Items,
    IReadOnlyList<ItemAggregatePlacement> ItemAggregates,
    IReadOnlyList<AgentPlacement> Agents);
