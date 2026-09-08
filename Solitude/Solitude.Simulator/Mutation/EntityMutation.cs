using Dapper;
using Microsoft.Data.Sqlite;
using Solitude.Simulator.Core.Model;

namespace Solitude.Simulator.Mutation;

public class EntityMutation(SqliteConnection connection)
{
    private readonly SqliteConnection _connection = connection;

    public long Create()
    {
        const string sql = """
              INSERT INTO entity DEFAULT VALUES
              RETURNING id;
              """;

        return _connection.QuerySingle<long>(sql);
    }

    public void Delete(long entityId)
    {
        const string sql = """
              DELETE FROM entity
              WHERE id = @EntityId;
              """;

        _connection.Execute(sql, new { EntityId = entityId });
    }

    public void SetAgent(EntityAgent agent)
    {
        const string sql = """
              INSERT INTO entity_agent (
                  entity_id,
                  name,
                  race,
                  drive,
                  size,
                  tiredness,
                  hunger
              )
              VALUES (
                  @EntityId,
                  @Name,
                  @Race,
                  @Drive,
                  @Size,
                  @Tiredness,
                  @Hunger
              )
              ON CONFLICT (entity_id) DO UPDATE SET
                  name = excluded.name,
                  race = excluded.race,
                  drive = excluded.drive,
                  size = excluded.size,
                  tiredness = excluded.tiredness,
                  hunger = excluded.hunger;
              """;

        _connection.Execute(sql, agent);
    }

    public void RemoveAgent(long entityId)
        => RemoveComponent("entity_agent", entityId);

    public void SetItem(EntityItem item)
    {
        const string sql = """
              INSERT INTO entity_item (
                  entity_id,
                  type,
                  quantity
              )
              VALUES (
                  @EntityId,
                  @Type,
                  @Quantity
              )
              ON CONFLICT (entity_id) DO UPDATE SET
                  type = excluded.type,
                  quantity = excluded.quantity;
              """;

        _connection.Execute(sql, item);
    }

    public void RemoveItem(long entityId)
        => RemoveComponent("entity_item", entityId);

    public void SetFeature(EntityFeature feature)
    {
        const string sql = """
              INSERT INTO entity_feature (
                  entity_id,
                  type
              )
              VALUES (
                  @EntityId,
                  @Type
              )
              ON CONFLICT (entity_id) DO UPDATE SET
                  type = excluded.type;
              """;

        _connection.Execute(sql, feature);
    }

    public void RemoveFeature(long entityId)
        => RemoveComponent("entity_feature", entityId);

    public void AddInventory(long entityId)
    {
        const string sql = """
              INSERT INTO entity_inventory (entity_id)
              VALUES (@EntityId);
              """;

        _connection.Execute(sql, new { EntityId = entityId });
    }

    public void RemoveInventory(long entityId)
        => RemoveComponent("entity_inventory", entityId);

    public void SetInventoryItem(EntityInventoryItem item)
    {
        const string sql = """
              INSERT INTO entity_inventory_item (
                  entity_id,
                  item_type,
                  quantity
              )
              VALUES (
                  @EntityId,
                  @ItemType,
                  @Quantity
              )
              ON CONFLICT (entity_id, item_type) DO UPDATE SET
                  quantity = excluded.quantity;
              """;

        _connection.Execute(sql, item);
    }

    public void RemoveInventoryItem(long entityId, Item item)
    {
        const string sql = """
              DELETE FROM entity_inventory_item
              WHERE entity_id = @EntityId
                AND item_type = @Item;
              """;

        _connection.Execute(
            sql,
            new
            {
                EntityId = entityId,
                Item = item
            });
    }

    public void SetPosition(EntityPosition position)
    {
        const string sql = """
              INSERT INTO entity_position (
                  entity_id,
                  map_x,
                  map_y,
                  x,
                  y
              )
              VALUES (
                  @EntityId,
                  @MapX,
                  @MapY,
                  @X,
                  @Y
              )
              ON CONFLICT (entity_id) DO UPDATE SET
                  map_x = excluded.map_x,
                  map_y = excluded.map_y,
                  x = excluded.x,
                  y = excluded.y;
              """;

        _connection.Execute(sql, position);
    }

    public void RemovePosition(long entityId)
        => RemoveComponent("entity_position", entityId);

    public void AddCollider(long entityId)
    {
        const string sql = """
              INSERT INTO entity_collider (
                  entity_id,
                  map_x,
                  map_y,
                  x,
                  y
              )
              SELECT
                  entity_id,
                  map_x,
                  map_y,
                  x,
                  y
              FROM entity_position
              WHERE entity_id = @EntityId
              ON CONFLICT (entity_id) DO UPDATE SET
                  map_x = excluded.map_x,
                  map_y = excluded.map_y,
                  x = excluded.x,
                  y = excluded.y;
              """;

        var affected = _connection.Execute(
            sql,
            new { EntityId = entityId });

        if (affected != 1)
        {
            throw new InvalidOperationException(
                "The entity must have a position before it can have a collider.");
        }
    }

    public void RemoveCollider(long entityId)
        => RemoveComponent("entity_collider", entityId);

    private void RemoveComponent(string table, long entityId)
    {
        _connection.Execute(
            $"DELETE FROM {table} WHERE entity_id = @EntityId;",
            new { EntityId = entityId });
    }
}
