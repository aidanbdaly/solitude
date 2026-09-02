# SQLite ECS schema review

`map.sql` is the first draft of an authoritative, relational entity-component-system schema. Work, item requirements, and item aggregates are intentionally out of scope for this version.

## Current model

`entity` provides global identity. All other entity tables are optional components whose `entity_id` is both their primary key and a foreign key to `entity`:

```text
entity
├── entity_agent
├── entity_item
├── entity_feature
├── entity_inventory
│   └── entity_inventory_item
├── entity_position
└── entity_collider
```

Component presence defines an entity; there is deliberately no entity-kind discriminator. An entity may possess any valid combination of components, and `GameContext` is responsible for enforcing game-specific combinations.

Only `entity.id` uses `AUTOINCREMENT`. Component tables reuse that ID, producing a single identity space whose committed IDs are never reused.

## Spatial state and collision

`entity_position` gives an entity at most one position and requires that position to reference an existing `map_tile`. Its coordinate index supports map, tile, and rendering queries without making coordinates generally unique, so multiple non-collidable entities may share a tile.

`entity_collider` is an optional component. It repeats the position columns so SQLite can enforce both of these invariants with ordinary keys:

- the collider coordinate must exactly match its entity's position;
- no two colliders may occupy the same coordinate.

The composite position foreign key uses `ON UPDATE CASCADE`, so moving a positioned collider updates both components atomically. A conflicting move fails through the collider coordinate's unique constraint. Deleting a position removes its collider.

Positions restrict deletion of their referenced tile. Consequently, deleting a populated tile or map fails until `GameContext` explicitly moves or deletes the affected entities; map deletion cannot silently leave unpositioned entities.

## Inventory and world items

`entity_inventory` is a capability marker, allowing the database to distinguish an empty inventory from an entity that cannot hold inventory. `entity_inventory_item` stores counted contents by item type and cascades when the capability is removed.

`entity_item` represents an individually identified item stack in the world. Picking it up should delete that entity and increment an inventory count in the same transaction. Dropping inventory should decrement the count and create a new entity with item and position components.

## Features

Features now have entity identity through `entity_feature`. Position and collision remain independent: a feature may be positioned without blocking movement, or receive an `entity_collider` component when it should block a tile.

## Persisted enums

The numeric `CHECK` constraints currently match the C# enum values. Those values are part of the persisted format and should be assigned explicitly in C#; reordering enum members must not change their stored meaning. Changes to the allowed values require a data-version migration.

## Context-enforced invariants

The database enforces identity, referential integrity, one position per entity, collider consistency, collision uniqueness, and basic value ranges. `GameContext` must still enforce:

- allowed component combinations;
- complete creation, pickup, drop, movement, and deletion transactions;

- complete rectangular map tile sets matching `width * height`;
- tile coordinates within their map dimensions;

- the meaning and progression of `data_version`;
- domain-specific limits for needs and quantities beyond the current non-negative checks.

## Connection and transaction requirements

Run this for every opened connection:

```sql
PRAGMA foreign_keys = ON;
```

The pragma in `map.sql` enables enforcement only for the connection executing the script. Every multi-component domain operation must run in one transaction so callers never observe partially constructed entities.

Create an entity and retrieve its ID with:

```sql
INSERT INTO entity DEFAULT VALUES
RETURNING id;
```

Do not read or modify SQLite's internal `sqlite_sequence` table.

## Remaining first-draft work

1. Add integration tests for every foreign key, unique constraint, cascade, and value check.
2. Add transactional tests for entity creation, movement, collision failure, pickup, drop, and deletion.
3. Validate complete map tile rectangles when maps are created and loaded.
4. Add work and item-requirement components in a later schema revision after their identity and spatial semantics are settled.

## Overall verdict

The schema now has a coherent relational ECS shape. Shared identity and position remove the duplicated agent/item placement structures, while collision and inventory are independent capabilities. The deliberate coordinate duplication in `entity_collider` is justified because it lets SQLite enforce collision atomically without triggers or a hard-coded collidable flag on every positioned entity.
