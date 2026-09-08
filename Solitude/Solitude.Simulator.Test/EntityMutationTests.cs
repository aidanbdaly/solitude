using Dapper;
using Solitude.Simulator.Core;
using Solitude.Simulator.Core.Model;
using Solitude.Simulator.Core.Mutation;
using Solitude.Simulator.Core.Query;
using Xunit;

public sealed class EntityMutationTests
{
    [Fact]
    public void CreatesSetsUpdatesAndRemovesComponents()
    {
        using var connection = SimulationDatabase.Create(":memory:");
        var mutation = new EntityMutation(connection);
        var query = new EntityQuery(connection);
        var entityId = mutation.Create();

        mutation.SetAgent(new EntityAgent
        {
            EntityId = entityId,
            Name = "Ada",
            Race = Agent.Human,
            Drive = AgentDrive.Acceptable,
            Size = AgentStature.AverageAlan,
            Tiredness = 0.25,
            Hunger = 0.5
        });
        mutation.SetItem(new EntityItem
        {
            EntityId = entityId,
            Type = Item.Wood,
            Quantity = 16
        });
        mutation.SetFeature(new EntityFeature
        {
            EntityId = entityId,
            Type = Feature.Rock
        });

        Assert.Equal(entityId, query.Get(entityId)?.Id);
        Assert.Equal("Ada", query.GetAgent(entityId)?.Name);
        Assert.Equal(16, query.GetItem(entityId)?.Quantity);
        Assert.Equal(Feature.Rock, query.GetFeature(entityId)?.Type);

        mutation.SetAgent(query.GetAgent(entityId)! with
        {
            Name = "Grace",
            Hunger = 0.75
        });
        mutation.SetItem(query.GetItem(entityId)! with
        {
            Type = Item.Stone,
            Quantity = 4
        });
        mutation.SetFeature(query.GetFeature(entityId)! with
        {
            Type = Feature.Flora
        });

        Assert.Equal("Grace", query.GetAgent(entityId)?.Name);
        Assert.Equal(0.75, query.GetAgent(entityId)?.Hunger);
        Assert.Equal(Item.Stone, query.GetItem(entityId)?.Type);
        Assert.Equal(4, query.GetItem(entityId)?.Quantity);
        Assert.Equal(Feature.Flora, query.GetFeature(entityId)?.Type);

        mutation.RemoveAgent(entityId);
        mutation.RemoveItem(entityId);
        mutation.RemoveFeature(entityId);

        Assert.Null(query.GetAgent(entityId));
        Assert.Null(query.GetItem(entityId));
        Assert.Null(query.GetFeature(entityId));
    }

    [Fact]
    public void AddsUpdatesAndRemovesInventoryItems()
    {
        using var connection = SimulationDatabase.Create(":memory:");
        var mutation = new EntityMutation(connection);
        var query = new EntityQuery(connection);
        var entityId = mutation.Create();

        mutation.AddInventory(entityId);
        mutation.SetInventoryItem(new EntityInventoryItem
        {
            EntityId = entityId,
            ItemType = Item.Wood,
            Quantity = 8
        });
        mutation.SetInventoryItem(new EntityInventoryItem
        {
            EntityId = entityId,
            ItemType = Item.Stone,
            Quantity = 3
        });
        mutation.SetInventoryItem(new EntityInventoryItem
        {
            EntityId = entityId,
            ItemType = Item.Wood,
            Quantity = 12
        });

        Assert.Equal(12, query.GetInventoryItem(entityId, Item.Wood)?.Quantity);
        Assert.Equal(2, query.GetInventoryItemSet(entityId).Count);

        mutation.RemoveInventoryItem(entityId, Item.Wood);

        Assert.Null(query.GetInventoryItem(entityId, Item.Wood));
        Assert.Single(query.GetInventoryItemSet(entityId));

        mutation.RemoveInventory(entityId);

        Assert.Null(query.GetInventory(entityId));
        Assert.Empty(query.GetInventoryItemSet(entityId));
    }

    [Fact]
    public void PositionControlsColliderCoordinatesAndLifetime()
    {
        using var connection = SimulationDatabase.Create(":memory:");
        CreateMap(connection);
        var mutation = new EntityMutation(connection);
        var query = new EntityQuery(connection);
        var entityId = mutation.Create();

        Assert.Throws<InvalidOperationException>(
            () => mutation.AddCollider(entityId));

        mutation.SetPosition(new EntityPosition
        {
            EntityId = entityId,
            MapX = 0,
            MapY = 0,
            X = 0,
            Y = 0
        });
        mutation.AddCollider(entityId);

        Assert.Equal(0, query.GetCollider(entityId)?.X);

        mutation.SetPosition(new EntityPosition
        {
            EntityId = entityId,
            MapX = 0,
            MapY = 0,
            X = 1,
            Y = 0
        });

        Assert.Equal(1, query.GetPosition(entityId)?.X);
        Assert.Equal(1, query.GetCollider(entityId)?.X);

        mutation.RemoveCollider(entityId);
        Assert.Null(query.GetCollider(entityId));

        mutation.AddCollider(entityId);
        mutation.RemovePosition(entityId);

        Assert.Null(query.GetPosition(entityId));
        Assert.Null(query.GetCollider(entityId));
    }

    [Fact]
    public void DeleteCascadesThroughComponents()
    {
        using var connection = SimulationDatabase.Create(":memory:");
        var mutation = new EntityMutation(connection);
        var query = new EntityQuery(connection);
        var entityId = mutation.Create();

        mutation.SetItem(new EntityItem
        {
            EntityId = entityId,
            Type = Item.Wood,
            Quantity = 1
        });
        mutation.AddInventory(entityId);
        mutation.SetInventoryItem(new EntityInventoryItem
        {
            EntityId = entityId,
            ItemType = Item.Stone,
            Quantity = 2
        });

        mutation.Delete(entityId);

        Assert.Null(query.Get(entityId));
        Assert.Null(query.GetItem(entityId));
        Assert.Null(query.GetInventory(entityId));
        Assert.Empty(query.GetInventoryItemSet(entityId));
    }

    private static void CreateMap(
        Microsoft.Data.Sqlite.SqliteConnection connection)
    {
        connection.Execute(
            """
            INSERT INTO map (x, y, width, height)
            VALUES (0, 0, 2, 1);

            INSERT INTO map_tile (map_x, map_y, x, y, type)
            VALUES
                (0, 0, 0, 0, 1),
                (0, 0, 1, 0, 1);
            """);
    }
}
