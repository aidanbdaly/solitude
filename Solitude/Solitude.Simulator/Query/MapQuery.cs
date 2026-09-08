using Dapper;
using Microsoft.Data.Sqlite;
using Solitude.Simulator.Core.Model;

namespace Solitude.Simulator.Query;

public class MapQuery(SqliteConnection connection)
{
    private readonly SqliteConnection _connection = connection;

    public Map? Get(int x, int y)
    {
        const string sql = """
              SELECT
                  x,
                  y,
                  width,
                  height
              FROM map
              WHERE x = @X
                AND y = @Y;
              """;

        return _connection.QuerySingleOrDefault<Map>(
            sql,
            new
            {
                X = x,
                Y = y
            });
    }

    public MapTile? GetTile(int mapX, int mapY, int tileX, int tileY)
    {
        const string sql = """
              SELECT
                  map_x,
                  map_y,
                  x,
                  y,
                  type
              FROM map_tile
              WHERE map_x = @MAPX
                AND map_y = @MAPY
                AND x = @TILEX
                AND y = @TILEY;
              """;

        return _connection.QuerySingleOrDefault<MapTile>(
            sql,
            new
            {
                MAPX = mapX,
                MAPY = mapY,
                TILEX = tileX,
                TILEY = tileY
            });
    }

    public IReadOnlyList<EntityPosition> GetEntity(int mapX, int mapY, int tileX, int tileY)
    {
        const string sql = """
              SELECT
                  entity_id,
                  map_x,
                  map_y,
                  x,
                  y
              FROM entity_position
              WHERE map_x = @MAPX
                AND map_y = @MAPY
                AND x = @TILEX
                AND y = @TILEY;
              """;

        return [.. _connection.Query<EntityPosition>(
            sql,
            new
            {
                MAPX = mapX,
                MAPY = mapY,
                TILEX = tileX,
                TILEY = tileY
            })];
    }

    public EntityCollider? GetEntityCollider(int mapX, int mapY, int tileX, int tileY)
    {
        const string sql = """
              SELECT
                  entity_id,
                  map_x,
                  map_y,
                  x,
                  y
              FROM entity_collider
              WHERE map_x = @MAPX
                AND map_y = @MAPY
                AND x = @TILEX
                AND y = @TILEY;
              """;

        return _connection.QuerySingleOrDefault<EntityCollider>(
            sql,
            new
            {
                MAPX = mapX,
                MAPY = mapY,
                TILEX = tileX,
                TILEY = tileY
            });
    }

    public IReadOnlyList<Map> GetSet()
    {
        const string sql = """
              SELECT
                  x,
                  y,
                  width,
                  height
              FROM map;
              """;

        return [.. _connection.Query<Map>(sql)];
    }

    public IReadOnlyList<MapTile> GetTileSet(int mapX, int mapY)
    {
        const string sql = """
              SELECT
                  map_x,
                  map_y,
                  x,
                  y,
                  type
              FROM map_tile
              WHERE map_x = @MAPX
                AND map_y = @MAPY;
              """;

        return [.. _connection.Query<MapTile>(
            sql,
            new
            {
                MAPX = mapX,
                MAPY = mapY
            })];
    }

    public IReadOnlyList<EntityPosition> GetEntitySet(int mapX, int mapY)
    {
        const string sql = """
              SELECT
                  entity_id,
                  map_x,
                  map_y,
                  x,
                  y
              FROM entity_position
              WHERE map_x = @MAPX
                AND map_y = @MAPY;
              """;

        return [.. _connection.Query<EntityPosition>(
            sql,
            new
            {
                MAPX = mapX,
                MAPY = mapY
            })];
    }

    public IReadOnlyList<EntityCollider> GetEntityColliderSet(int mapX, int mapY)
    {
        const string sql = """
              SELECT
                  entity_id,
                  map_x,
                  map_y,
                  x,
                  y
              FROM entity_collider
              WHERE map_x = @MAPX
                AND map_y = @MAPY;
              """;

        return [.. _connection.Query<EntityCollider>(
            sql,
            new
            {
                MAPX = mapX,
                MAPY = mapY
            })];
    }
}
