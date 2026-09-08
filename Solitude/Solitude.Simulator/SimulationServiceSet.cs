namespace Solitude.Simulator;

public sealed class SimulationServiceSet
{
    private readonly Dictionary<Type, object> _services = [];

    public void Add<T>(T service)
       where T : notnull
    {
        _services.Add(typeof(T), service);
    }

    public T Get<T>()
       where T : notnull
    {
        if (_services.TryGetValue(typeof(T), out var service))
        {
            return (T)service;
        }

        throw new InvalidOperationException(
            $"Service '{typeof(T).FullName}' is not registered.");
    }
}