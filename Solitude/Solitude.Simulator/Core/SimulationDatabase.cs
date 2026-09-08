using System.Reflection;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Solitude.Simulator.Core;

public static class SimulationDatabase
{
    public const int CurrentSchemaVersion = 4;

    private const string SchemaResourceName = "Solitude.Simulator.Core.Sql.map.sql";

    public static SqliteConnection Create(string databasePath)
    {
        ValidatePath(databasePath);

        var inMemory = databasePath == ":memory:";
        if (!inMemory && File.Exists(databasePath))
        {
            throw new IOException($"Database '{databasePath}' already exists.");
        }

        SqliteConnection? connection = null;

        try
        {
            connection = OpenConnection(
                databasePath,
                inMemory ? SqliteOpenMode.Memory : SqliteOpenMode.ReadWriteCreate);

            using var transaction = connection.BeginTransaction();
            connection.Execute(ReadSchema(), transaction: transaction);
            transaction.Commit();

            return connection;
        }
        catch
        {
            connection?.Dispose();
            throw;
        }
    }

    public static SqliteConnection Open(string databasePath)
    {
        ValidatePath(databasePath);

        if (databasePath != ":memory:" && !File.Exists(databasePath))
        {
            throw new FileNotFoundException("Database does not exist.", databasePath);
        }

        SqliteConnection? connection = null;

        try
        {
            connection = OpenConnection(
                databasePath,
                databasePath == ":memory:" ? SqliteOpenMode.Memory : SqliteOpenMode.ReadWrite);

            var version = connection.QuerySingle<int>("PRAGMA user_version;");
            if (version != CurrentSchemaVersion)
            {
                throw new InvalidDataException(
                    $"Unsupported database schema version '{version}'. " +
                    $"Expected '{CurrentSchemaVersion}'.");
            }

            return connection;
        }
        catch (InvalidDataException)
        {
            connection?.Dispose();
            throw;
        }
        catch (SqliteException exception)
        {
            connection?.Dispose();
            throw new InvalidDataException("Database is not a valid Solitude database.", exception);
        }
        catch
        {
            connection?.Dispose();
            throw;
        }
    }

    private static SqliteConnection OpenConnection(string databasePath, SqliteOpenMode mode)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = mode
        }.ToString());

        connection.Open();
        connection.Execute("PRAGMA foreign_keys = ON;");
        return connection;
    }

    private static string ReadSchema()
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(SchemaResourceName)
            ?? throw new InvalidOperationException(
                $"Embedded schema resource '{SchemaResourceName}' was not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static void ValidatePath(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
    }
}
