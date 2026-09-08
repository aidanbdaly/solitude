using Dapper;
using Microsoft.Data.Sqlite;
using Solitude.Simulator.Core.Model;

namespace Solitude.Simulator.Query;

public class EntityQuery(SqliteConnection connection)
{
    private readonly SqliteConnection _connection = connection;

    public Entity? Get(long entityId)
    {
        const string sql = """
              SELECT id
              FROM entity
              WHERE id = @EntityId;
              """;

        return _connection.QuerySingleOrDefault<Entity>(
            sql,
            new { EntityId = entityId });
    }

    public EntityAgent? GetAgent(long entityId)
    {
        const string sql = """
              SELECT
                  entity_id,
                  name,
                  race,
                  drive,
                  size,
                  tiredness,
                  hunger
              FROM entity_agent
              WHERE entity_id = @EntityId;
              """;

        return _connection.QuerySingleOrDefault<EntityAgent>(
            sql,
            new { EntityId = entityId });
    }

    public EntityItem? GetItem(long entityId)
    {
        const string sql = """
              SELECT
                  entity_id,
                  type,
                  quantity
              FROM entity_item
              WHERE entity_id = @EntityId;
              """;

        return _connection.QuerySingleOrDefault<EntityItem>(
            sql,
            new { EntityId = entityId });
    }

    public EntityFeature? GetFeature(long entityId)
    {
        const string sql = """
              SELECT
                  entity_id,
                  type
              FROM entity_feature
              WHERE entity_id = @EntityId;
              """;

        return _connection.QuerySingleOrDefault<EntityFeature>(
            sql,
            new { EntityId = entityId });
    }

    public EntityInventory? GetInventory(long entityId)
    {
        const string sql = """
              SELECT entity_id
              FROM entity_inventory
              WHERE entity_id = @EntityId;
              """;

        return _connection.QuerySingleOrDefault<EntityInventory>(
            sql,
            new { EntityId = entityId });
    }

    public EntityInventoryItem? GetInventoryItem(long entityId, Item item)
    {
        const string sql = """
              SELECT
                  entity_id,
                  item_type,
                  quantity
              FROM entity_inventory_item
              WHERE entity_id = @EntityId
                AND item_type = @Item;
              """;

        return _connection.QuerySingleOrDefault<EntityInventoryItem>(
            sql,
            new
            {
                EntityId = entityId,
                Item = item
            });
    }

    public EntityPosition? GetPosition(long entityId)
    {
        const string sql = """
              SELECT
                  entity_id,
                  map_x,
                  map_y,
                  x,
                  y
              FROM entity_position
              WHERE entity_id = @EntityId;
              """;

        return _connection.QuerySingleOrDefault<EntityPosition>(
            sql,
            new { EntityId = entityId });
    }

    public EntityCollider? GetCollider(long entityId)
    {
        const string sql = """
              SELECT
                  entity_id,
                  map_x,
                  map_y,
                  x,
                  y
              FROM entity_collider
              WHERE entity_id = @EntityId;
              """;

        return _connection.QuerySingleOrDefault<EntityCollider>(
            sql,
            new { EntityId = entityId });
    }

    public IReadOnlyList<EntityInventoryItem> GetInventoryItemSet(long entityId)
    {
        const string sql = """
              SELECT
                  entity_id,
                  item_type,
                  quantity
              FROM entity_inventory_item
              WHERE entity_id = @EntityId
              ORDER BY item_type;
              """;

        return [.. _connection.Query<EntityInventoryItem>(
            sql,
            new { EntityId = entityId })];
    }
}
