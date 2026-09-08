using Microsoft.Data.Sqlite;
using Solitude.Simulator.Core;
using Solitude.Simulator.Mutation;
using Solitude.Simulator.Query;
using Solitude.Simulator.Service;

namespace Solitude.Simulator;

public sealed class Simulation : IDisposable
{
    public const uint CycleLength = 600;

    public const uint CycleStart = 200;

    private readonly SqliteConnection _connection;

    private readonly SimulationServiceSet _service = new();

    private Simulation(SqliteConnection connection)
    {
        _connection = connection;

        _service.Add<MetaQuery>(new(_connection));
        _service.Add<MetaMutation>(new(_connection));
        _service.Add<MapQuery>(new(_connection));
        _service.Add<MapMutation>(new(_connection));
        _service.Add<EntityQuery>(new(_connection));
        _service.Add<EntityMutation>(new(_connection));
    }

    /// <summary>
    /// Compute the next frame of the simulation by flushing the mutation buffer
    /// </summary>
    public void Next()
    {
        // MovementProcedure.Execute()
    }

    public static Simulation Create(SimulationParameters request)
    {
        var connection = SimulationDatabase.Create(request.Path);

        var simulation = new Simulation(connection);

        // Enqueue statements for a single transaction handled by next
        simulation.Get<ScenarioService>().GenerateScenario(request.Scenario);
        simulation.Get<MetaMutation>().Create(request.Scenario.MapX, request.Scenario.MapY);
        // Why? Invariant violation handling centralized around one method, no half-valid frames:
        // you either will get a valid next frame or you won't.

        simulation.Next();

        return simulation;
    }

    public static Simulation Open(string path)
    {
        var connection = SimulationDatabase.Open(path);

        var simulation = new Simulation(connection);

        return simulation;
    }


    // Remove create map
    // rename Metamuation
    // move seed on to meta

    public T Get<T>() where T : notnull => _service.Get<T>();

    public void Dispose() => _connection.Dispose();
}
