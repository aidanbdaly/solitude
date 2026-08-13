using System;
using Solitude.Domain.Game;

namespace Solitude.Domain.Game.Agents;

public sealed class NeedsProcess
{
    private readonly World _world;

    public NeedsProcess(World world)
    {
        _world = world;
    }

    public void Step(float delta)
    {
        if (!float.IsFinite(delta) || delta < 0f)
            throw new ArgumentOutOfRangeException(nameof(delta));

        var elapsedDays = delta / Clock.RealSecondsPerDay;
        foreach (var agent in _world.Agents)
        {
            agent.Needs.Advance(
                agent.Definition.TirednessPerDay * elapsedDays,
                agent.Definition.SatiationConsumedPerDay * elapsedDays);
        }
    }
}
