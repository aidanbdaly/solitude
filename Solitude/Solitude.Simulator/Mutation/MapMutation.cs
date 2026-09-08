using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Solitude.Simulator.Request;

namespace Solitude.Simulator.Mutation;

public class MapMutation(SqliteConnection connection)
{
    private readonly SqliteConnection _connection = connection;

    public void Create(CreateMap request)
    {
        using var transaction = _connection.BeginTransaction();

        Create(request, transaction);

        transaction.Commit();
    }

    internal void Create(
        CreateMap request,
        IDbTransaction transaction)
    {
        Validate(request);

        const string insertMap = """
              INSERT INTO map (
                  x,
                  y,
                  width,
                  height
              )
              VALUES (
                  @X,
                  @Y,
                  @Width,
                  @Height
              );
              """;

        const string insertTile = """
              INSERT INTO map_tile (
                  map_x,
                  map_y,
                  x,
                  y,
                  type
              )
              VALUES (
                  @MapX,
                  @MapY,
                  @X,
                  @Y,
                  @Type
              );
              """;

        var tiles = request.Tiles.Select((type, index) => new
        {
            MapX = request.X,
            MapY = request.Y,
            X = index % request.Width,
            Y = index / request.Width,
            Type = type
        });

        _connection.Execute(
            insertMap,
            request,
            transaction);

        _connection.Execute(
            insertTile,
            tiles,
            transaction);
    }

    private static void Validate(CreateMap request)
    {
        if (request.Tiles.Count != request.Width * request.Height)
        {
            throw new ArgumentOutOfRangeException(
              nameof(request),
              "Map must contain exactly width x height tiles.");
        }

        if (request.X < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.X),
                "Map X must be non-negative.");
        }

        if (request.Y < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Y),
                "Map Y must be non-negative.");
        }

        if (request.Width <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Width),
                "Map width must be positive.");
        }

        if (request.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Height),
                "Map height must be positive.");
        }
    }
}
