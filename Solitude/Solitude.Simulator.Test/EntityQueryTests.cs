using Dapper;
using Solitude.Simulator.Core;
using Solitude.Simulator.Core.Model;
using Solitude.Simulator.Core.Query;
using Xunit;

public sealed class EntityQueryTests
{
    [Fact]
    public void GetsEntityAndEachComponent()
    {
        using var connection = SimulationDatabase.Create(":memory:");
        var entityId = CreateEntityWithComponents(connection);
        var query = new EntityQuery(connection);

        Assert.Equal(new Entity { Id = entityId }, query.Get(entityId));
        Assert.Equal(
            new EntityAgent
            {
                EntityId = entityId,
                Name = "Ada",
                Race = Agent.Human,
                Drive = AgentDrive.Acceptable,
                Size = AgentStature.AverageAlan,
                Tiredness = 0.25,
                Hunger = 0.5
            },
            query.GetAgent(entityId));
        Assert.Equal(
            new EntityItem
            {
                EntityId = entityId,
                Type = Item.Wood,
                Quantity = 16
            },
            query.GetItem(entityId));
        Assert.Equal(
            new EntityFeature
            {
                EntityId = entityId,
                Type = Feature.Rock
            },
            query.GetFeature(entityId));
        Assert.Equal(
            new EntityInventory { EntityId = entityId },
            query.GetInventory(entityId));
        Assert.Equal(
            new EntityPosition
            {
                EntityId = entityId,
                MapX = 0,
                MapY = 0,
                X = 0,
                Y = 0
            },
            query.GetPosition(entityId));
        Assert.Equal(
            new EntityCollider
            {
                EntityId = entityId,
                MapX = 0,
                MapY = 0,
                X = 0,
                Y = 0
            },
            query.GetCollider(entityId));
    }

    [Fact]
    public void GetsSpecificInventoryItemAndOrderedSet()
    {
        using var connection = SimulationDatabase.Create(":memory:");
        var entityId = CreateEntityWithComponents(connection);
        var query = new EntityQuery(connection);

        Assert.Equal(
            new EntityInventoryItem
            {
                EntityId = entityId,
                ItemType = Item.Stone,
                Quantity = 3
            },
            query.GetInventoryItem(entityId, Item.Stone));
        Assert.Equal(
            [
                new EntityInventoryItem
                {
                    EntityId = entityId,
                    ItemType = Item.Wood,
                    Quantity = 8
                },
                new EntityInventoryItem
                {
                    EntityId = entityId,
                    ItemType = Item.Stone,
                    Quantity = 3
                }
            ],
            query.GetInventoryItemSet(entityId));
    }

    [Fact]
    public void MissingEntityAndComponentsReturnNoResults()
    {
        using var connection = SimulationDatabase.Create(":memory:");
        var query = new EntityQuery(connection);

        Assert.Null(query.Get(1));
        Assert.Null(query.GetAgent(1));
        Assert.Null(query.GetItem(1));
        Assert.Null(query.GetFeature(1));
        Assert.Null(query.GetInventory(1));
        Assert.Null(query.GetInventoryItem(1, Item.Wood));
        Assert.Null(query.GetPosition(1));
        Assert.Null(query.GetCollider(1));
        Assert.Empty(query.GetInventoryItemSet(1));
    }

    private static long CreateEntityWithComponents(
        Microsoft.Data.Sqlite.SqliteConnection connection)
    {
        connection.Execute(
            """
            INSERT INTO map (x, y, width, height)
            VALUES (0, 0, 1, 1);

            INSERT INTO map_tile (map_x, map_y, x, y, type)
            VALUES (0, 0, 0, 0, 1);
            """);

        var entityId = connection.QuerySingle<long>(
            "INSERT INTO entity DEFAULT VALUES RETURNING id;");

        connection.Execute(
            """
            INSERT INTO entity_agent (
                entity_id, name, race, drive, size, tiredness, hunger
            ) VALUES (
                @EntityId, 'Ada', 0, 1, 1, 0.25, 0.5
            );

            INSERT INTO entity_item (entity_id, type, quantity)
            VALUES (@EntityId, 0, 16);

            INSERT INTO entity_feature (entity_id, type)
            VALUES (@EntityId, 1);

            INSERT INTO entity_inventory (entity_id)
            VALUES (@EntityId);

            INSERT INTO entity_inventory_item (entity_id, item_type, quantity)
            VALUES
                (@EntityId, 1, 3),
                (@EntityId, 0, 8);

            INSERT INTO entity_position (entity_id, map_x, map_y, x, y)
            VALUES (@EntityId, 0, 0, 0, 0);

            INSERT INTO entity_collider (entity_id, map_x, map_y, x, y)
            VALUES (@EntityId, 0, 0, 0, 0);
            """,
            new { EntityId = entityId });

        return entityId;
    }
}
