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
        if (delta == 0f)
            return;

        var elapsedDays = delta / Clock.RealSecondsPerDay;
        foreach (var agent in _world.Agents)
        {
            var tirednessIncrease = agent.Definition.TirednessPerDay * elapsedDays;
            if (tirednessIncrease > 0f)
                _world.IncreaseAgentTiredness(agent.Id, tirednessIncrease);

            var satiationDecrease = agent.Definition.SatiationConsumedPerDay * elapsedDays;
            if (satiationDecrease > 0f)
                _world.DecreaseAgentSatiation(agent.Id, satiationDecrease);
        }
    }
}
