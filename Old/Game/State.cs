using System;

namespace Solitude.Domain.Game;

public sealed class State
{
    public World World { get; }
    public Clock Clock { get; }

    public State(World world, Clock clock)
    {
        World = world ?? throw new ArgumentNullException(nameof(world));
        Clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }
}
