using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Solitude.Simulator.Core;
using Solitude.Simulator.Core.Model;
using Xunit;

public sealed class GameDatabaseTests
{
    [Fact]
    public void CreateInitializesEmbeddedSchemaAndConnectionSettings()
    {
        using var connection = SimulationDatabase.Create(":memory:");

        var tables = connection.Query<string>(
            """
            SELECT name
            FROM sqlite_schema
            WHERE type = 'table'
              AND name NOT LIKE 'sqlite_%'
            ORDER BY name;
            """).ToArray();

        Assert.Equal(SimulationDatabase.CurrentSchemaVersion, connection.QuerySingle<int>("PRAGMA user_version;"));
        Assert.Equal(1, connection.QuerySingle<int>("PRAGMA foreign_keys;"));
        Assert.Equal(
            [
                "entity",
                "entity_agent",
                "entity_collider",
                "entity_feature",
                "entity_inventory",
                "entity_inventory_item",
                "entity_item",
                "entity_position",
                "map",
                "map_tile",
                "meta"
            ],
            tables);
    }

    [Fact]
    public void OpenReopensCreatedDatabaseAndRetainsState()
    {
        var path = NewDatabasePath();

        try
        {
            using (var connection = SimulationDatabase.Create(path))
            {
                connection.Execute("INSERT INTO entity DEFAULT VALUES;");
            }

            using var reopened = SimulationDatabase.Open(path);

            Assert.Equal(1, reopened.QuerySingle<int>("SELECT count(*) FROM entity;"));
            Assert.Equal(1, reopened.QuerySingle<int>("PRAGMA foreign_keys;"));
        }
        finally
        {
            DeleteIfPresent(path);
        }
    }

    [Fact]
    public void CreateRejectsExistingDatabaseWithoutOverwritingIt()
    {
        var path = NewDatabasePath();

        try
        {
            File.WriteAllText(path, "existing");

            Assert.Throws<IOException>(() => SimulationDatabase.Create(path));
            Assert.Equal("existing", File.ReadAllText(path));
        }
        finally
        {
            DeleteIfPresent(path);
        }
    }

    [Fact]
    public void OpenRejectsMissingDatabase()
    {
        var path = NewDatabasePath();

        Assert.Throws<FileNotFoundException>(() => SimulationDatabase.Open(path));
    }

    [Fact]
    public void OpenRejectsEmptyDatabase()
    {
        var path = NewDatabasePath();

        try
        {
            File.WriteAllBytes(path, []);

            Assert.Throws<InvalidDataException>(() => SimulationDatabase.Open(path));
        }
        finally
        {
            DeleteIfPresent(path);
        }
    }

    [Fact]
    public void OpenRejectsMalformedDatabase()
    {
        var path = NewDatabasePath();

        try
        {
            File.WriteAllText(path, "not a sqlite database");

            Assert.Throws<InvalidDataException>(() => SimulationDatabase.Open(path));
        }
        finally
        {
            DeleteIfPresent(path);
        }
    }

    [Fact]
    public void OpenRejectsUnsupportedSchemaVersion()
    {
        var path = NewDatabasePath();

        try
        {
            using (var connection = new SqliteConnection($"Data Source={path}"))
            {
                connection.Open();
                connection.Execute("PRAGMA user_version = 5;");
            }

            Assert.Throws<InvalidDataException>(() => SimulationDatabase.Open(path));
        }
        finally
        {
            DeleteIfPresent(path);
        }
    }

    [Fact]
    public void ForeignKeysRejectOrphanComponents()
    {
        using var connection = SimulationDatabase.Create(":memory:");

        Assert.Throws<SqliteException>(() => connection.Execute(
            "INSERT INTO entity_feature (entity_id, type) VALUES (1, 0);"));
    }

    [Fact]
    public void DapperMapsComponentRecord()
    {
        using var connection = SimulationDatabase.Create(":memory:");
        var entityId = connection.QuerySingle<long>(
            "INSERT INTO entity DEFAULT VALUES RETURNING id;");

        connection.Execute(
            """
            INSERT INTO entity_agent (
                entity_id, name, race, drive, size, tiredness, hunger
            )
            VALUES (
                @EntityId, @Name, @Race, @Drive, @Size, @Tiredness, @Hunger
            );
            """,
            new
            {
                EntityId = entityId,
                Name = "Ada",
                Race = Agent.Human,
                Drive = AgentDrive.Acceptable,
                Size = AgentStature.AverageAlan,
                Tiredness = 0.25,
                Hunger = 0.5
            });

        var component = connection.QuerySingle<EntityAgent>(
            """
            SELECT
                entity_id AS EntityId,
                name AS Name,
                race AS Race,
                drive AS Drive,
                size AS Size,
                tiredness AS Tiredness,
                hunger AS Hunger
            FROM entity_agent
            WHERE entity_id = @entityId;
            """,
            new { entityId });

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
            component);
    }

    [Fact]
    public void DisposeClosesConnectionAndIsIdempotent()
    {
        var connection = SimulationDatabase.Create(":memory:");

        connection.Dispose();
        connection.Dispose();

        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    private static string NewDatabasePath()
        => Path.Combine(Path.GetTempPath(), $"solitude-{Guid.NewGuid():N}.db");

    private static void DeleteIfPresent(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
