using Dapper;
using Microsoft.Data.Sqlite;

namespace Solitude.Simulator.Mutation;

public class MetaMutation(SqliteConnection connection)
{
    private readonly SqliteConnection _connection = connection;

    public void Create(long mapX, long mapY)
    {
        const string insertMeta = """
              INSERT INTO meta (
                  singleton,
                  data_version,
                  elapsed,
                  active_map_x,
                  active_map_y
              )
              VALUES (
                  1,
                  1,
                  0,
                  @X,
                  @Y
              );
              """;

        using var transaction = _connection.BeginTransaction();

        _connection.Execute(
            insertMeta,
            new
            {
                X = mapX,
                Y = mapY
            },
            transaction);

        transaction.Commit();
    }

    public void SetActiveMap(int mapX, int mapY)
    {
        if (mapX < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(mapX),
                "Map X must be non-negative.");
        }

        if (mapY < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(mapY),
                "Map Y must be non-negative.");
        }

        const string sql = """
          UPDATE meta
          SET active_map_x = @MapX,
              active_map_y = @MapY
          WHERE singleton = 1;
          """;

        var affected = _connection.Execute(
            sql,
            new
            {
                MapX = mapX,
                MapY = mapY
            });

        if (affected != 1)
        {
            throw new InvalidOperationException(
                "The metadata has not been created.");
        }
    }

    public void AdvanceElapsed(long amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Elapsed amount must be non-negative.");
        }

        const string sql = """
          UPDATE meta
          SET elapsed = elapsed + @Amount
          WHERE singleton = 1;
          """;

        var affected = _connection.Execute(
            sql,
            new { Amount = amount });

        if (affected != 1)
        {
            throw new InvalidOperationException(
                "The metadata has not been created.");
        }
    }
}
