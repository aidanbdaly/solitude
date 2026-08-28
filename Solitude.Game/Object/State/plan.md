# Save schema plan

## Goal

Keep the runtime model encapsulated and serialize an explicit, versioned snapshot of
its durable state. Runtime events, caches, Godot nodes, derived occupancy data, and
simulation services are not part of the save format.

`Game`, `World`, and `Map` remain behavioural domain objects. They create snapshots
for saving and reconstruct themselves from validated snapshots when loading.

## Reusing existing types

An existing type can appear directly in the save schema when all of the following
are true:

- It is an immutable value or definition rather than a mutable entity.
- Every member is intentionally part of the durable file format.
- Its members are themselves persistence-safe.
- Its serialized shape can be kept compatible or migrated when it changes.

This means a separate `AgentDefinitionSave` is not required. `AgentDefinition` can
be stored directly after its public fields are changed to `required` `init`
properties so that it is actually immutable after construction.

The same approach applies to small value types such as `Tile`, `ItemRequirement`,
`MapId`, `WorldAddress`, and the enum types. Numeric enum values must remain stable;
otherwise the schema should serialize explicit string or numeric codes instead.

Mutable entities and implementation containers do need snapshot representations.
`Agent`, `Item`, `Map`, `World`, `Grid<T>`, and `SparseGrid<T>` must not be serialized
directly.

## Indexes and identity

- Agent and item IDs are stable `long` values. They may be sparse, are never list
  offsets, and are not reused.
- `World` owns ID-keyed entity dictionaries and reverse address indexes.
- Maps own only the forward spatial indexes from coordinate to entity. A global
  entity ID must never index storage sized from map dimensions.
- Map placement records are the authoritative persisted location data. Entity
  dictionaries, reverse address indexes, and occupancy indexes are rebuilt and
  validated from those records during restoration rather than serialized.
- Restoration creates canonical entities first, then resolves placement IDs and
  rejects duplicate, missing, or multiply placed entities.

## Top-level schema

Keep the related snapshot records together in one `SaveSchema.cs` file initially;
there is no need for one file per record.

```csharp
public sealed record GameSave(
    int Version,
    PlayerSave Player,
    WorldSave World,
    MapId? ActiveMapId);

public sealed record WorldSave(
    uint Width,
    uint Height,
    long NextAgentId,
    long NextItemId,
    IReadOnlyList<AgentSave> Agents,
    IReadOnlyList<ItemSave> Items,
    IReadOnlyList<MapSave> Maps);

public sealed record MapSave(
    MapId Id,
    int Width,
    int Height,
    uint Time,
    IReadOnlyList<Tile> Tiles,
    IReadOnlyList<PlacedFeatureSave> Features,
    IReadOnlyList<PlacedWorkSave> Work,
    IReadOnlyList<PlacedItemSave> Items,
    IReadOnlyList<PlacedItemAggregateSave> ItemAggregates,
    IReadOnlyList<PlacedAgentSave> Agents);
```

`PlayerSave` can remain empty or be omitted from `GameSave` until `Player` contains
durable state.

## Entity snapshots

World-owned entities are serialized once. Map placement records refer to their IDs
instead of embedding duplicate object graphs.

```csharp
public sealed record AgentSave(
    long Id,
    AgentDefinition Definition,
    AgentStatusSave Status,
    InventorySave Inventory);

public sealed record ItemSave(
    long Id,
    ItemType Type,
    int Count);

public sealed record PlacedAgentSave(Vector2I Coordinate, long AgentId);
public sealed record PlacedItemSave(Vector2I Coordinate, long ItemId);
public sealed record PlacedFeatureSave(Vector2I Coordinate, FeatureType Type);
```

`AgentStatusSave` and `InventorySave` represent mutable runtime state. If
`AgentStatus` is deliberately converted into an immutable value object with a
stable persistence contract, it may replace `AgentStatusSave` directly.

Work and item aggregates need records containing their actual mutable progress and
contents, not only their public read-only projections. Their exact records should
be finalized when mutation operations for those types are implemented.

## Grid representation

- Store the dense tile grid as a row-major `Tiles` list with exactly
  `Width * Height` entries.
- Store sparse layers as coordinate/value placement lists.
- Do not save `_occupation`; rebuild it from feature, work, item, aggregate, and
  agent placements while restoring the map.
- Validate all coordinates, duplicate placements, tile counts, IDs, and references
  before constructing the runtime model.

`Vector2I` should use a small explicit JSON converter or be replaced at the schema
boundary by a stable coordinate record such as `CellSave(int X, int Y)`. Do not rely
on Godot's internal JSON representation as the save-file contract.

## Conversion boundary

Add snapshot operations to the aggregate roots and relevant contained types:

```csharp
public GameSave CreateSave();
public static Game Restore(GameSave save);
```

`World` and `Map` should have corresponding internal `CreateSave` and `Restore`
operations. Restore methods should populate private fields through constructors or
controlled internal methods; they should not expose mutable collections publicly.

`Save.Write` serializes `game.CreateSave()`. `Save.Read` deserializes `GameSave`,
checks `Version`, validates it, and calls `Game.Restore(save)`.

## Versioning and validation

- Start with schema version `1`.
- Reject unsupported future versions with a clear error.
- Add explicit migrations when an older schema can be upgraded safely.
- Reject negative counts, invalid dimensions, duplicate IDs, missing entity
  references, invalid active-map IDs, and out-of-bounds coordinates.
- Recalculate next IDs from the maximum restored IDs, or validate persisted next-ID
  values before accepting them.
- Never serialize events or restore event subscribers.

## Implementation order

1. Make reusable definition/value types genuinely immutable and confirm their JSON
   representation.
2. Add `SaveSchema.cs` and a converter for coordinates.
3. Implement snapshots and restoration for inventory, agents, and items.
4. Implement map snapshots, sparse placements, and occupancy reconstruction.
5. Implement world and game snapshots.
6. Change `Save.Read` and `Save.Write` to use `GameSave`.
7. Add round-trip tests and malformed-save validation tests.
8. Add at least one version-migration test before changing version `1`.
