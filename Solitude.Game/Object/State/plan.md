# Save schema plan

## Goal

Serialize an explicit, versioned snapshot of the durable state owned by `Game`,
`World`, and `Map`. Keep runtime models encapsulated and exclude events, Godot
nodes, caches, derived occupancy, and other runtime-only indexes.

The save schema mirrors the current state model without serializing its private
implementation containers directly.

## Current ownership model

- `Game` owns an empty `Player`, one `World`, and an optional active world
  coordinate.
- `World` owns its dimensions, maps, canonical agents, canonical items, the next
  agent ID, and agent/item address indexes.
- `Map` owns its dimensions, time, dense tiles, and sparse feature, work, item,
  aggregate, and agent layers.
- Agents own an immutable `AgentDefinition`, an init-only `AgentStatus`, and a
  mutable `AgentInventory`.
- Items are world-owned entities identified by stable `long` IDs; maps contain
  references to them.
- Features, work, and item aggregates are map-owned values and do not have IDs.
- `AgentNavigation` is not currently attached to an agent and is therefore not
  part of the save graph.

## Schema

Keep the related records together in `Snapshots.cs` under the
`Solitude.Persistence` namespace. Use persistence-only coordinates so the JSON
contract does not depend on Godot's representation of `Vector2I`.

```csharp
namespace Solitude.Persistence;

public readonly record struct Coordinate(int X, int Y);

public sealed record GameSnapshot(
    int Version,
    WorldSnapshot World,
    Coordinate? ActiveWorldCoordinate)
{
    public const int CurrentVersion = 1;
}

public sealed record WorldSnapshot(
    uint Width,
    uint Height,
    long NextAgentId,
    long NextItemId,
    IReadOnlyList<AgentSnapshot> Agents,
    IReadOnlyList<ItemSnapshot> Items,
    IReadOnlyList<MapSnapshot> Maps);

public sealed record MapSnapshot(
    Coordinate WorldCoordinate,
    int Width,
    int Height,
    uint Time,
    IReadOnlyList<Tile> Tiles,
    IReadOnlyList<FeaturePlacement> Features,
    IReadOnlyList<WorkPlacement> Work,
    IReadOnlyList<ItemPlacement> Items,
    IReadOnlyList<ItemAggregatePlacement> ItemAggregates,
    IReadOnlyList<AgentPlacement> Agents);

public sealed record AgentSnapshot(
    long Id,
    AgentDefinition Definition,
    AgentStatus Status,
    IReadOnlyList<ItemCount> Inventory);

public sealed record ItemSnapshot(long Id, ItemType Type, int Count);
public sealed record ItemCount(ItemType Type, int Count);

public sealed record AgentPlacement(Coordinate Coordinate, long AgentId);
public sealed record ItemPlacement(Coordinate Coordinate, long ItemId);
public sealed record FeaturePlacement(Coordinate Coordinate, FeatureType Type);

public sealed record WorkPlacement(
    Coordinate Coordinate,
    int Required,
    int Current);

public sealed record ItemAggregatePlacement(
    Coordinate Coordinate,
    IReadOnlyList<ItemRequirement> Required,
    IReadOnlyList<ItemCount> Contents);
```

`Player` is omitted until it contains durable state. `AgentDefinition`,
`AgentStatus`, `Tile`, and `ItemRequirement` can be reused because their current
public state is immutable after construction and intentionally belongs in the save
contract. Mutable inventory and aggregate dictionaries become item-count lists so
duplicate keys and invalid counts can be validated explicitly.

Items are included in version 1 because `World` owns their allocation, stable IDs,
canonical instances, and placements. The `Snapshot` suffix is reserved for captures
of mutable domain objects; structural values and placements use ordinary nouns.

## Identity, indexes, and grids

- Agent and item IDs are stable `long` values and are never interpreted as list
  offsets.
- Canonical agents and items are serialized once; map placements refer to IDs.
- Map placement records are the authoritative persisted location data.
- World address dictionaries and map occupancy are rebuilt from placements and are
  not serialized independently.
- Restore canonical entities before resolving placements. Reject duplicate IDs,
  missing references, multiple placements for one entity, duplicate coordinates,
  and coordinates outside their map or world bounds.
- Persist tiles in row-major order with exactly `Width * Height` entries.
- Persist sparse layers as coordinate/value placement lists.
- Persist `NextAgentId` and `NextItemId` so deleted or sparse IDs are not reused;
  require each counter to be greater than every corresponding restored ID.

## Conversion boundary

Keep conversion in static extension factories under `Solitude.Game/Persistence`:

```csharp
public static GameSnapshot ToSnapshot(this Game game);
internal static Game ToGame(this GameSnapshot snapshot);
internal static WorldSnapshot ToSnapshot(this World world);
internal static World ToWorld(this WorldSnapshot snapshot);
internal static MapSnapshot ToSnapshot(this Map map, Coordinate worldCoordinate);
internal static Map ToMap(this MapSnapshot snapshot);
```

State classes must not reference snapshot types. They expose narrowly scoped,
internal domain-state queries and construction hooks so the factories can enumerate
state and rebuild complete aggregates without making those operations part of the
public gameplay API.

Restoration should:

1. Validate schema version, dimensions, counts, IDs, and coordinates.
2. Construct the world and each empty map.
3. Restore dense tiles and map-owned feature, work, and aggregate values.
4. Restore canonical agents and items into the world dictionaries.
5. Resolve item and agent placement IDs, populate map grids, and rebuild address
   and occupancy indexes.
6. Validate and restore the active world coordinate without emitting runtime
   change events.

`Save.Write` serializes `game.ToSnapshot()`. `Save.Read` deserializes
`GameSnapshot`, validates `GameSnapshot.CurrentVersion`, and calls
`snapshot.ToGame()`. Opening either read or write files must be checked before
dereferencing the Godot `FileAccess` result.

## Validation and tests

- Reject unsupported versions, zero or oversized dimensions, tile-count
  mismatches, negative item/work counts, work progress above its requirement,
  duplicate IDs, invalid next IDs, missing placement references, duplicate or
  multiply occupied coordinates, and invalid active-map coordinates.
- Add a complete round-trip test containing time, non-default tiles, features,
  work progress, an aggregate with contents, agents with inventory/status, items,
  and placements across more than one map.
- Add malformed-save tests for every structural validation category.
- Verify restored objects are canonical: a placed agent/item is the same object
  stored in the world's ID dictionary.
- Add a migration test before introducing schema version `2`.

## Implementation order

1. Add the schema records and coordinate conversions.
2. Implement the map snapshot extension factory.
3. Implement the world snapshot extension factory, placement resolution, and index rebuilding.
4. Implement the game snapshot extension factory.
5. Switch `Save.Read` and `Save.Write` to the versioned schema.
6. Add round-trip and malformed-save tests.
