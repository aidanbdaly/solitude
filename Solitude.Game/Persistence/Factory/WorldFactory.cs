using System;
using System.Collections.Generic;
using System.Linq;

namespace Solitude.Persistence;

internal static class WorldSnapshotFactory
{
    internal static WorldSnapshot ToSnapshot(this World world)
    {
        var agents = world.Agents.Values
            .OrderBy(agent => agent.Id)
            .Select(agent => new AgentSnapshot(
                agent.Id,
                agent.Definition,
                agent.Status,
                agent.Inventory
                    .OrderBy(entry => entry.Key)
                    .Select(entry => new ItemCount(entry.Key, entry.Value))
                    .ToArray()))
            .ToArray();

        var items = world.Items.Values
            .OrderBy(item => item.Id)
            .Select(item => new ItemSnapshot(item.Id, item.Type, item.Count))
            .ToArray();

        var maps = world.GetMaps()
            .Select(entry => entry.Map.ToSnapshot(entry.Coordinate.ToSnapshot()))
            .ToArray();

        return new(
            world.Width,
            world.Height,
            world.NextAgentId,
            world.NextItemId,
            agents,
            items,
            maps);
    }

    internal static World ToWorld(this WorldSnapshot snapshot)
    {
        SnapshotValidation.Require(snapshot is not null, "World snapshot is missing");
        SnapshotValidation.Require(snapshot.Width > 0 && snapshot.Height > 0, "World dimensions must be positive");
        SnapshotValidation.Require(
            snapshot.Width <= int.MaxValue && snapshot.Height <= int.MaxValue &&
            (ulong)snapshot.Width * snapshot.Height <= int.MaxValue,
            "World dimensions are too large");

        var world = new World(snapshot.Width, snapshot.Height);
        var mapSnapshots = SnapshotValidation.RequireList(snapshot.Maps, nameof(snapshot.Maps));
        var mapCoordinates = new HashSet<Coordinate>();

        foreach (var mapSnapshot in mapSnapshots)
        {
            SnapshotValidation.Require(mapSnapshot is not null, "Map snapshot is missing");
            ValidateWorldCoordinate(mapSnapshot.WorldCoordinate, snapshot.Width, snapshot.Height);
            SnapshotValidation.Require(mapCoordinates.Add(mapSnapshot.WorldCoordinate), "Map coordinates must be unique");
            world.AddMap(mapSnapshot.WorldCoordinate.ToVector2I(), mapSnapshot.ToMap());
        }

        long maximumAgentId = -1;
        foreach (var agentSnapshot in SnapshotValidation.RequireList(snapshot.Agents, nameof(snapshot.Agents)))
        {
            SnapshotValidation.Require(agentSnapshot is not null, "Agent snapshot is missing");
            SnapshotValidation.Require(agentSnapshot.Id >= 0, "Agent ID cannot be negative");
            SnapshotValidation.Require(agentSnapshot.Definition is not null, "Agent definition is missing");
            SnapshotValidation.Require(agentSnapshot.Status is not null, "Agent status is missing");
            SnapshotValidation.Require(!string.IsNullOrWhiteSpace(agentSnapshot.Definition.Name), "Agent name is missing");
            SnapshotValidation.Require(Enum.IsDefined(agentSnapshot.Definition.Type), "Agent type is invalid");
            SnapshotValidation.Require(Enum.IsDefined(agentSnapshot.Definition.Drive), "Agent drive is invalid");
            SnapshotValidation.Require(Enum.IsDefined(agentSnapshot.Definition.Stature), "Agent stature is invalid");
            SnapshotValidation.Require(
                float.IsFinite(agentSnapshot.Status.Tiredness) && float.IsFinite(agentSnapshot.Status.Hunger),
                "Agent status values must be finite");

            var inventory = new AgentInventory();
            var inventoryTypes = new HashSet<ItemType>();
            foreach (var count in SnapshotValidation.RequireList(agentSnapshot.Inventory, nameof(agentSnapshot.Inventory)))
            {
                SnapshotValidation.Require(count is not null, "Inventory count is missing");
                ValidateItemCount(count, inventoryTypes, "inventory");
                inventory.Add(count.Type, count.Count);
            }

            try
            {
                world.AddAgent(new()
                {
                    Id = agentSnapshot.Id,
                    Definition = agentSnapshot.Definition,
                    Status = agentSnapshot.Status,
                    Inventory = inventory
                });
            }
            catch (ArgumentException exception)
            {
                throw new System.IO.InvalidDataException($"Duplicate agent ID '{agentSnapshot.Id}'", exception);
            }

            maximumAgentId = Math.Max(maximumAgentId, agentSnapshot.Id);
        }

        long maximumItemId = -1;
        foreach (var itemSnapshot in SnapshotValidation.RequireList(snapshot.Items, nameof(snapshot.Items)))
        {
            SnapshotValidation.Require(itemSnapshot is not null, "Item snapshot is missing");
            SnapshotValidation.Require(itemSnapshot.Id >= 0, "Item ID cannot be negative");
            SnapshotValidation.Require(Enum.IsDefined(itemSnapshot.Type), "Item type is invalid");
            SnapshotValidation.Require(itemSnapshot.Count >= 0, "Item count cannot be negative");

            try
            {
                world.AddItem(new()
                {
                    Id = itemSnapshot.Id,
                    Type = itemSnapshot.Type,
                    Count = itemSnapshot.Count
                });
            }
            catch (ArgumentException exception)
            {
                throw new System.IO.InvalidDataException($"Duplicate item ID '{itemSnapshot.Id}'", exception);
            }

            maximumItemId = Math.Max(maximumItemId, itemSnapshot.Id);
        }

        SnapshotValidation.Require(
            snapshot.NextAgentId >= 0 && snapshot.NextAgentId > maximumAgentId,
            "Next agent ID must exceed every restored agent ID");
        SnapshotValidation.Require(
            snapshot.NextItemId >= 0 && snapshot.NextItemId > maximumItemId,
            "Next item ID must exceed every restored item ID");

        var placedAgents = new HashSet<long>();
        var placedItems = new HashSet<long>();

        foreach (var mapSnapshot in mapSnapshots)
        {
            var worldCoordinate = mapSnapshot.WorldCoordinate.ToVector2I();

            foreach (var placement in SnapshotValidation.RequireList(mapSnapshot.Agents, nameof(mapSnapshot.Agents)))
            {
                SnapshotValidation.Require(placement is not null, "Agent placement is missing");
                SnapshotValidation.Require(
                    SnapshotValidation.Contains(placement.Coordinate, mapSnapshot.Width, mapSnapshot.Height),
                    $"Agent placement '{placement.Coordinate}' is out of bounds");
                SnapshotValidation.Require(world.Agents.ContainsKey(placement.AgentId),
                    $"Agent placement references unknown ID '{placement.AgentId}'");
                SnapshotValidation.Require(placedAgents.Add(placement.AgentId),
                    $"Agent '{placement.AgentId}' is placed more than once");

                world.PlaceAgent(
                    placement.AgentId,
                    new(worldCoordinate, placement.Coordinate.ToVector2I()));
            }

            foreach (var placement in SnapshotValidation.RequireList(mapSnapshot.Items, nameof(mapSnapshot.Items)))
            {
                SnapshotValidation.Require(placement is not null, "Item placement is missing");
                SnapshotValidation.Require(
                    SnapshotValidation.Contains(placement.Coordinate, mapSnapshot.Width, mapSnapshot.Height),
                    $"Item placement '{placement.Coordinate}' is out of bounds");
                SnapshotValidation.Require(world.Items.ContainsKey(placement.ItemId),
                    $"Item placement references unknown ID '{placement.ItemId}'");
                SnapshotValidation.Require(placedItems.Add(placement.ItemId),
                    $"Item '{placement.ItemId}' is placed more than once");

                world.PlaceItem(
                    placement.ItemId,
                    new(worldCoordinate, placement.Coordinate.ToVector2I()));
            }
        }

        SnapshotValidation.Require(placedAgents.Count == world.Agents.Count, "Every agent must have one placement");
        SnapshotValidation.Require(placedItems.Count == world.Items.Count, "Every item must have one placement");

        world.SetNextIds(snapshot.NextAgentId, snapshot.NextItemId);
        return world;
    }

    private static void ValidateWorldCoordinate(Coordinate coordinate, uint width, uint height)
        => SnapshotValidation.Require(
            coordinate.X >= 0 && coordinate.Y >= 0 &&
            (uint)coordinate.X < width && (uint)coordinate.Y < height,
            $"World coordinate '{coordinate}' is out of bounds");

    private static void ValidateItemCount(ItemCount count, HashSet<ItemType> types, string owner)
    {
        SnapshotValidation.Require(Enum.IsDefined(count.Type), $"Invalid item type '{count.Type}'");
        SnapshotValidation.Require(count.Count >= 0, $"{owner} item count cannot be negative");
        SnapshotValidation.Require(types.Add(count.Type), $"{owner} item types must be unique");
    }
}
