using Dapper;
using Microsoft.Data.Sqlite;
using Solitude.Simulator.Core.Model;

namespace Solitude.Simulator.Query;

public class MetaQuery(SqliteConnection connection)
{
    private readonly SqliteConnection _connection = connection;

    public long GetElapsed()
    {
        const string sql = """
              SELECT elapsed
              FROM meta
              WHERE meta.singleton = 1;
              """;

        return _connection.QuerySingle<long>(sql);
    }

    public Map GetActiveMap()
    {
        const string sql = """
              SELECT
                  map.x,
                  map.y,
                  map.width,
                  map.height
              FROM meta
              INNER JOIN map
                  ON map.x = meta.active_map_x
                 AND map.y = meta.active_map_y
              WHERE meta.singleton = 1;
              """;

        return _connection.QuerySingle<Map>(sql);
    }
}
