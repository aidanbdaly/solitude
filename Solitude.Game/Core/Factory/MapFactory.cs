using System;
using System.Collections.Generic;
using System.Linq;

namespace Solitude.Persistence;

internal static class MapSnapshotFactory
{
    internal static MapSnapshot ToSnapshot(this Map map, Coordinate worldCoordinate)
    {
        var tiles = new List<Tile>(checked(map.Width * map.Height));
        foreach (var (_, tile) in map.Tile)
        {
            tiles.Add(tile);
        }

        var features = new List<FeaturePlacement>();
        foreach (var (coordinate, feature) in map.Feature)
        {
            features.Add(new(coordinate, feature.Type));
        }

        var work = new List<WorkPlacement>();
        foreach (var (coordinate, value) in map.Work)
        {
            work.Add(new(coordinate, value.GetRequired(), value.Get()));
        }

        var items = new List<ItemPlacement>();
        foreach (var (coordinate, item) in map.Item)
        {
            items.Add(new(coordinate, item.Id));
        }

        var aggregates = new List<ItemAggregatePlacement>();
        foreach (var (coordinate, aggregate) in map.Aggregate)
        {
            aggregates.Add(new(
                coordinate,
                aggregate.Required.ToArray(),
                aggregate
                    .OrderBy(entry => entry.Key)
                    .Select(entry => new ItemCount(entry.Key, entry.Value))
                    .ToArray()));
        }

        var agents = new List<AgentPlacement>();
        foreach (var (coordinate, agent) in map.Agent)
        {
            agents.Add(new(coordinate, agent.Id));
        }

        return new(
            worldCoordinate,
            map.Width,
            map.Height,
            map.GetTime(),
            tiles,
            features,
            work,
            items,
            aggregates,
            agents);
    }

    internal static Map ToMap(this MapSnapshot snapshot)
    {
        SnapshotValidation.Require(snapshot is not null, "Map snapshot is missing");
        SnapshotValidation.Require(snapshot.Width > 0 && snapshot.Height > 0, "Map dimensions must be positive");
        SnapshotValidation.Require(
            (long)snapshot.Width * snapshot.Height <= int.MaxValue,
            "Map dimensions are too large");

        var tiles = SnapshotValidation.RequireList(snapshot.Tiles, nameof(snapshot.Tiles));
        SnapshotValidation.Require(
            tiles.Count == checked(snapshot.Width * snapshot.Height),
            "Map tile count does not match its dimensions");

        var map = new Map((uint)snapshot.Width, (uint)snapshot.Height);
        var tileIndex = 0;

        for (var y = 0; y < snapshot.Height; y++)
        {
            for (var x = 0; x < snapshot.Width; x++)
            {
                var tile = tiles[tileIndex++];
                SnapshotValidation.Require(Enum.IsDefined(tile.Type), $"Invalid tile type '{tile.Type}'");
                map.SetTile(tile, new(x, y));
            }
        }

        foreach (var placement in SnapshotValidation.RequireList(snapshot.Features, nameof(snapshot.Features)))
        {
            SnapshotValidation.Require(placement is not null, "Feature placement is missing");
            ValidateCoordinate(placement.Coordinate, snapshot.Width, snapshot.Height);
            SnapshotValidation.Require(Enum.IsDefined(placement.Type), $"Invalid feature type '{placement.Type}'");
            map.SetFeature(new(placement.Type), placement.Coordinate);
        }

        foreach (var placement in SnapshotValidation.RequireList(snapshot.Work, nameof(snapshot.Work)))
        {
            SnapshotValidation.Require(placement is not null, "Work placement is missing");
            ValidateCoordinate(placement.Coordinate, snapshot.Width, snapshot.Height);
            SnapshotValidation.Require(placement.Required > 0, "Work requirement must be positive");
            SnapshotValidation.Require(
                placement.Current >= 0 && placement.Current <= placement.Required,
                "Work progress must be between zero and its requirement");

            var value = new Work(placement.Required);
            value.Set(placement.Current);
            map.SetWork(value, placement.Coordinate);
        }

        foreach (var placement in SnapshotValidation.RequireList(snapshot.ItemAggregates, nameof(snapshot.ItemAggregates)))
        {
            SnapshotValidation.Require(placement is not null, "Item aggregate placement is missing");
            ValidateCoordinate(placement.Coordinate, snapshot.Width, snapshot.Height);

            var required = SnapshotValidation.RequireList(placement.Required, nameof(placement.Required));
            var requiredTypes = new HashSet<ItemType>();
            foreach (var requirement in required)
            {
                SnapshotValidation.Require(requirement is not null, "Item requirement is missing");
                SnapshotValidation.Require(Enum.IsDefined(requirement.Type), $"Invalid item type '{requirement.Type}'");
                SnapshotValidation.Require(requirement.Count >= 0, "Item requirement cannot be negative");
                SnapshotValidation.Require(requiredTypes.Add(requirement.Type), "Item requirement types must be unique");
            }

            var aggregate = new ItemAggregate(required.ToList());
            var contentTypes = new HashSet<ItemType>();
            foreach (var content in SnapshotValidation.RequireList(placement.Contents, nameof(placement.Contents)))
            {
                SnapshotValidation.Require(content is not null, "Item aggregate content is missing");
                SnapshotValidation.Require(Enum.IsDefined(content.Type), $"Invalid item type '{content.Type}'");
                SnapshotValidation.Require(content.Count >= 0, "Item aggregate content cannot be negative");
                SnapshotValidation.Require(contentTypes.Add(content.Type), "Item aggregate content types must be unique");
                aggregate.Add(content.Type, content.Count);
            }

            map.SetItemAggregate(aggregate, placement.Coordinate);
        }

        map.SetTime(snapshot.Time);
        return map;
    }

    private static void ValidateCoordinate(Coordinate coordinate, int width, int height)
        => SnapshotValidation.Require(
            SnapshotValidation.Contains(coordinate, width, height),
            $"Map coordinate '{coordinate}' is out of bounds");
}
